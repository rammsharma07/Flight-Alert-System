using Application.Interfaces.Email;
using Application.Interfaces.Meeting;
using Azure.Messaging.EventHubs.Consumer;
using Domain;
using Domain.Email;
using Domain.Meeting;
using Domain.Response;
using Domain.Response.EventHub;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using Persistance;
using Persistance.Interfaces.Meeting;
using SendGrid.Helpers.Mail;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;
using Application.Interfaces.SMS;
using Web.Models;
using SendGrid.Helpers.Mail.Model;
using Newtonsoft.Json.Linq;
using Persistance.CommonFunctions;
using Application.Interfaces.Utils;
using Domain.EntityHistory;
using System.Reflection;
using Application.Interfaces.EntityHistory;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Application.Services.Meeting
{
	public class MeetingUserServices(IMeetingUserRepository meetingUserRepository, IMeetingsService meetingsService, ISmsServices smsServices, IEntityHistoryService _historyService, DapperContext context, IConfiguration configuration, IEmailService emailService, IUserFlightStatusRepository userFlightStatusRepository, IMeetingsRepository meetingsRepository) : IMeetingUserServices
	{
		private readonly IMeetingUserRepository meetingUserRepository = meetingUserRepository;
		private readonly IMeetingsService meetingsService = meetingsService;
		private readonly DapperContext _context = context;
		private readonly IConfiguration _configuration = configuration;
		private readonly IEmailService _emailService = emailService;
		private readonly ISmsServices _smsServices = smsServices;
		private readonly IUserFlightStatusRepository userFlightStatusRepository = userFlightStatusRepository;
		private readonly IMeetingsRepository _meetingsRepository = meetingsRepository;



		private static readonly string[] RequiredColumns = ["MeetingName", "FirstName", "LastName", "EmailId", "PhoneNumber", "AttendeeType", "CarrierCode", "FlightNumber", "DepartureDate", "DepartureTime", "ArrivalDate", "ArrivalTime", "OriginAirport", "DestinationAirport", "FlightLabel", "Connecting"];

		private static readonly string[] RequiredProperties = { "MeetingName", "AttendeeType", "FlightNumber", "DepartureDate", "DepartureTime", "ArrivalDate", "ArrivalTime", "CarrierCode", "OriginAirport", "DestinationAirport", "FlightLabel", "Connecting", "ID" };

		/// <summary>
		/// Add Meeting User
		/// </summary>
		/// <param name="meetingUser"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> AddMeetingUserAsync(MeetingUser meetingUser)
		{

			Log.Information("AddMeetingUserAsync Function is running {DateTime.UtcNow}, ", DateTime.UtcNow);
			var checkExist = await CheckMeetingUser(meetingUser, null);

			Log.Information("CheckMeetingUser Successfully run {DateTime.UtcNow}, ", DateTime.UtcNow);
			if (checkExist.Data.Count > 0)
			{
				return new Response<MeetingUser>
				{
					Message = Constants.DuplicateRecord,
					Status = HttpStatusCode.NotAcceptable
				};
			}

			Log.Information("meetingUserRepository insert function call {DateTime.UtcNow}, ", DateTime.UtcNow);
			return await this.meetingUserRepository.Insert(meetingUser);
		}

		/// <summary>
		/// Add Meeting Details
		/// </summary>
		/// <param name="formFile"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> AddMeetingDetailsAsync(IFormFile formFile, int userId, int selectedMeetingID)
		{
			var (listMeetingUser, errorMessage) = await ExtractDataFromExcelAsync(formFile, userId, selectedMeetingID);
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				return new Response<MeetingUser>
				{
					Message = errorMessage,
					Status = HttpStatusCode.BadRequest
				};
			}
			var distinctMeetingIds = listMeetingUser
	.Select(mu => mu.MeetingID)
	.Distinct()
	.ToList();
			var listUserMeeting = await meetingUserRepository.MeetingBulkInsert(listMeetingUser).ConfigureAwait(false);
			try
			{
				if (listUserMeeting.Status == HttpStatusCode.OK)
				{

					var insertedattendees = listUserMeeting.Data
	.SelectMany(x => x)
	.ToList();

					await InsertBulkCreateHistoryAsync(insertedattendees, userId);

				}
			}
			catch (Exception ex)
			{
				Log.Information("User Id {DateTime.UtcNow}, userId :{userId}, Excaption in Activity log", DateTime.UtcNow, userId, ex);
			}
			return new Response<MeetingUser>
			{
				Message = listUserMeeting.Message,
				Status = listUserMeeting.Status,
				Extra = distinctMeetingIds
			};
		}

		/// <summary>
		/// InsertBulkCreateHistoryAsync
		/// </summary>
		/// <param name="users"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		private async Task InsertBulkCreateHistoryAsync(List<MeetingUser> users, int userId)
		{
			foreach (var user in users)
			{
				var historyResponse =
					await _historyService.InsertEntityAuditHistory(
						new EntityHistoryAuditDto
						{
							MeetingId = user.MeetingID,
							AttendeeId = user.Id,
							Module = "Attendees",
							SubModule = "Attendee",
							Action = "Create",
							EntityAffected = $"{user.FirstName} {user.LastName}",
							User = userId,
							CreatedDate = DateTime.UtcNow,
							Description = "Attendee created via bulk upload"
						});

				if (historyResponse.Status != HttpStatusCode.OK)
					continue;

				int entityHistoryId = historyResponse.entityHistoryId;

				var fieldChanges = GeneralFunctions.BuildCreateFieldHistory(
					entityHistoryId,
					user,
					userId);

				await _historyService
					.InsertEntityFieldHistoryChanges(fieldChanges);
			}
		}


		/// <summary>
		/// Is Excel File Valid
		/// </summary>
		/// <param name="formFile"></param>
		/// <returns></returns>
		private static bool IsExcelFileValid(IFormFile formFile)
		{
			return Path.GetExtension(formFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
				   Path.GetExtension(formFile.FileName).Equals(".xls", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Extract Data From Excel
		/// </summary>
		/// <param name="formFile"></param>
		/// <returns></returns>
		private async Task<(List<MeetingUser>, string)> ExtractDataFromExcelAsync(IFormFile formFile, int userId, int selectedMeetingID)
		{
			ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
			string errorMessage = "";
			var listMeetingUser = new List<MeetingUser>();
			try
			{
				#region File validation

				if (formFile == null || formFile.Length == 0)
				{
					errorMessage = "Please select a file to upload..";
					return (listMeetingUser, errorMessage);
				}
				if (formFile.Length > 5 * 1024 * 1024)
				{
					errorMessage = "File size must be less than 5 MB.";
					return (listMeetingUser, errorMessage);
				}
				if (!IsExcelFileValid(formFile))
				{
					errorMessage = "Invalid file format. Please upload an Excel file in .xlsx, .xls format.";
					return (listMeetingUser, errorMessage);
				}

				#endregion

				using (var stream = new MemoryStream())
				{
					await formFile.CopyToAsync(stream);
					using (var package = new ExcelPackage(stream))
					{
						ExcelWorksheet workSheet = package.Workbook.Worksheets[0];

						for (int row = workSheet.Dimension.End.Row; row >= workSheet.Dimension.Start.Row; row--)
						{
							bool isRowNull = true;
							// Check if all cells in the row are null
							for (int col = workSheet.Dimension.Start.Column; col <= workSheet.Dimension.End.Column; col++)
							{
								if (workSheet.Cells[row, col].Value != null)
								{
									isRowNull = false;
									break;
								}
							}
							// If all cells in the row are null, remove the row
							if (isRowNull)
								workSheet.DeleteRow(row);
						}
						#region work Sheet Validation

						if (workSheet.Dimension == null || workSheet.Dimension.Rows <= 1)
						{
							errorMessage = "The uploaded Excel file is empty or contains no data.";
							return (listMeetingUser, errorMessage);
						}
						if (!IsHeaderValid(workSheet))
						{
							errorMessage = "The uploaded Excel file does not contain the required columns.";
							return (listMeetingUser, errorMessage);
						}

                        #endregion

                        #region Mandatory columns null check
                        for (int row = 2; row <= workSheet.Dimension.Rows; row++)
                        {
                            var meetingName = Convert.ToString(workSheet.Cells[row, 1].Value);
                            var firstName = Convert.ToString(workSheet.Cells[row, 2].Value);
                            var lastName = Convert.ToString(workSheet.Cells[row, 3].Value);
                            var email = Convert.ToString(workSheet.Cells[row, 4].Value);
                            var phone = Convert.ToString(workSheet.Cells[row, 5].Value);

                            if (string.IsNullOrWhiteSpace(meetingName) ||
                                string.IsNullOrWhiteSpace(firstName) ||
                                string.IsNullOrWhiteSpace(lastName) ||
                                string.IsNullOrWhiteSpace(email) ||
                                string.IsNullOrWhiteSpace(phone))
                            {
                                return (new List<MeetingUser>(), $"Mandatory columns are null. Row No. {row}");
                            }
                        }
                        #endregion

                        string ID = "";
						for (int row = 2; row <= workSheet.Dimension.Rows; row++)
						{
							MeetingUser meetingUser = new()
							{
								FirstName = Convert.ToString(workSheet.Cells[row, 2].Value),
								LastName = Convert.ToString(workSheet.Cells[row, 3].Value),
								EmailId = Convert.ToString(workSheet.Cells[row, 4].Value),
								PhoneNumber = Convert.ToString(workSheet.Cells[row, 5].Value),
								AttendeeType = Convert.ToString(workSheet.Cells[row, 6].Value),
								CarrierCode = Convert.ToString(workSheet.Cells[row, 7].Value),
								DepartureFlightNumber = Convert.ToString(workSheet.Cells[row, 8].Value),
								OriginAirport = Convert.ToString(workSheet.Cells[row, 13].Value),
								DestinationAirport = Convert.ToString(workSheet.Cells[row, 14].Value),
								FlightType = Convert.ToString(workSheet.Cells[row, 15].Value)

							};
							ID = Convert.ToString(workSheet.Cells[row, 17].Value);
                       
                            #region Connecting Validation
                            try
                            {
								var cellValue = Convert.ToString(workSheet.Cells[row, 16].Value);

								if (string.IsNullOrWhiteSpace(cellValue))
								{
									meetingUser.Connecting = false;
								}
								else
								{
									var normalized = cellValue.Trim();

									meetingUser.Connecting =
										normalized.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
										normalized.Equals("Y", StringComparison.OrdinalIgnoreCase) ||
										normalized.Equals("True", StringComparison.OrdinalIgnoreCase);
								}
							}
							catch (Exception)
							{
								errorMessage = $"Connecting value is not valid. Please check Row No. {row}.";
								return (listMeetingUser, errorMessage);
							}
							#endregion

							#region Departure DateTime
							try
							{
								var depDateVal = workSheet.Cells[row, 9].Value;
								var depTimeVal = workSheet.Cells[row, 10].Value;

								DateTime depDate;
								DateTime depTime;

								if (depDateVal is DateTime dtDate)
								{
									depDate = dtDate;
								}
								else if (double.TryParse(depDateVal?.ToString(), out double dDate))
								{
									depDate = DateTime.FromOADate(dDate);
								}
								else if (!DateTime.TryParse(depDateVal?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out depDate))
								{
									errorMessage = $"Departure date is not valid. Please check Row No. {row}.";
									return (listMeetingUser, errorMessage);
								}

								if (depTimeVal is DateTime dtTime)
								{
									depTime = dtTime;
								}
								else if (double.TryParse(depTimeVal?.ToString(), out double dTime))
								{
									depTime = DateTime.FromOADate(dTime);
								}
								else if (!DateTime.TryParse(depTimeVal?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out depTime))
								{
									depTime = DateTime.MinValue; 
								}

								meetingUser.DepartureDateTime = depDate.Date.Add(depTime.TimeOfDay);
							}
							catch
							{
								errorMessage = $"Departure date/time is not valid. Please check Row No. {row}.";
								return (listMeetingUser, errorMessage);
							}
							#endregion

							#region Arrival DateTime
							try
							{
								var arrDateVal = workSheet.Cells[row, 11].Value;
								var arrTimeVal = workSheet.Cells[row, 12].Value;

								DateTime arrDate;
								DateTime arrTime;

								if (arrDateVal is DateTime dtDate)
								{
									arrDate = dtDate;
								}
								else if (double.TryParse(arrDateVal?.ToString(), out double dDate))
								{
									arrDate = DateTime.FromOADate(dDate);
								}
								else if (!DateTime.TryParse(arrDateVal?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out arrDate))
								{
									errorMessage = $"Arrival date is not valid. Please check Row No. {row}.";
									return (listMeetingUser, errorMessage);
								}

								if (arrTimeVal is DateTime dtTime)
								{
									arrTime = dtTime;
								}
								else if (double.TryParse(arrTimeVal?.ToString(), out double dTime))
								{
									arrTime = DateTime.FromOADate(dTime);
								}
								else if (!DateTime.TryParse(arrTimeVal?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out arrTime))
								{
									arrTime = DateTime.MinValue; 
								}

								meetingUser.ArrivalDateTime = arrDate.Date.Add(arrTime.TimeOfDay);
							}
							catch
							{
								errorMessage = $"Arrival date/time is not valid. Please check Row No. {row}.";
								return (listMeetingUser, errorMessage);
							}
							#endregion


							#region Check modal Null validation

							foreach (var propertyColumnName in RequiredColumns)
							{
								if (!RequiredProperties.Contains(propertyColumnName))
								{
									string propertyName = GetPropertieName(propertyColumnName);

									var property = typeof(MeetingUser).GetProperty(propertyName);
									var value = property.GetValue(meetingUser);
									if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
									{
										var displayNameAttribute = typeof(MeetingUser).GetProperty(propertyName).GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;

										string displayName = propertyName;
										if (displayNameAttribute != null)
											displayName = displayNameAttribute.DisplayName;

										errorMessage = $"The {displayName} field cannot be null or empty. Please check Row No. {row}.";
										return (listMeetingUser, errorMessage);
									}
								}
							}

							#endregion
							#region Check Flight number
							string flightNumberStr = Convert.ToString(workSheet.Cells[row, 8].Value);
							if (!int.TryParse(flightNumberStr, out int flightNumber))
							{
								errorMessage = $"Invalid flight number. Please check Row No. {row}.";
								return (listMeetingUser, errorMessage);
							}
							meetingUser.DepartureFlightNumber = flightNumberStr;
							#endregion

							#region string, number and email validation

							if (!ValidateEmail(meetingUser.EmailId))
							{
								errorMessage = $"Invalid email format. Please check Row No. {row}.";
								return (listMeetingUser, errorMessage);
							}


							meetingUser.Meetings = new Meetings
							{
								MeetingName = Convert.ToString(workSheet.Cells[row, 1].Value)
							};
							if (string.IsNullOrEmpty(meetingUser.Meetings.MeetingName))
							{
								errorMessage = $"The MeetingName field cannot be null or empty. Please check Row No. {row}.";
								return (listMeetingUser, errorMessage);
							}

							Meetings meetings = new()
							{
								MeetingName = meetingUser.Meetings.MeetingName,
								IsActive = true,
								IsDeleted = false,
								EmailState = Constants.Canceled,
								EmailStatus = Constants.Delayed,
								SmsState = Constants.Canceled,
								SmsStatus = Constants.Delayed
							};

							var listMeeting = await meetingsService.GetMeetingsDetails(meetings, selectedMeetingID).ConfigureAwait(false);
							if (listMeeting.Data.Count == 0)
							{
								if (string.IsNullOrEmpty(ID))
								{
									meetings.CreatedBy = userId;
									listMeeting = await meetingsService.AddMeetingsAsync(meetings).ConfigureAwait(false);
									ActivityMeetingDetails activityMeetingDetails = new()
									{
										MeetingId = listMeeting.Data[0].Id,
										MeetingName = meetingUser.Meetings.MeetingName,
										IsActive = true,
										IsDeleted = false,
										updatedby = userId,
										dateTime = DateTime.UtcNow,
										Module = "Meeting",
										Action = "Create",
										Description = "Meeting create sucessfully"
									};

									await meetingsRepository.InsertMeetingEntityHistory(activityMeetingDetails).ConfigureAwait(false);
								}
								else
								{
									errorMessage = $"Invalid Meeting Name, Please check Row No. {row}.";
									return (listMeetingUser, errorMessage);
								}
							}
							
							meetingUser.MeetingID = listMeeting.Data[0].Id;



							#endregion
							meetingUser.CreatedBy = userId;
							if (meetingUser.CarrierCode != null)
							{
								string carriercode = meetingUser.CarrierCode;
								meetingUser.CarrierCode = GeneralFunctions.GetCarriercode(carriercode);
							}
							var checkExist = await CheckMeetingUser(meetingUser, ID);
							if (checkExist.Data.Count == 0)
							{
								listMeetingUser.Add(meetingUser);
							}
							else
							{

								int attendeeId = 0;
								MeetingUser oldAttendee = null;

								if (!string.IsNullOrWhiteSpace(ID))
								{
									if (!int.TryParse(ID, out attendeeId))
									{
										errorMessage = $"Invalid ID value '{ID}'. Please check Row No. {row}.";
										return (listMeetingUser, errorMessage);
									}
								}

								var oldResponse = await GetMeetingUserById(attendeeId).ConfigureAwait(false);

								if (oldResponse != null &&
									oldResponse.Status == HttpStatusCode.OK &&
									oldResponse.Data != null)
								{
									oldAttendee = oldResponse.Data.FirstOrDefault();
								}
								var updateResult = await UpdateRecords(meetingUser, ID);
								try
								{
									if (updateResult.Status == HttpStatusCode.OK && oldAttendee != null)
									{
										var changedFields = GeneralFunctions.GetChangedFields(oldAttendee, meetingUser);

										var summary = GeneralFunctions.BuildChangeSummary("Update", changedFields);

										var historyResponse = await _historyService.InsertEntityAuditHistory(
										new EntityHistoryAuditDto
										{
											MeetingId = meetingUser.MeetingID,
											AttendeeId = attendeeId,
											Module = "Attendees",
											SubModule = "Attendee",
											Action = "Update",
											EntityAffected = oldAttendee.FirstName + ' ' + oldAttendee.LastName,
											User = userId,
											CreatedDate = DateTime.UtcNow,
											Description = summary
										});
										if (historyResponse.Status == HttpStatusCode.OK)
										{
											int entityHistoryId = historyResponse.entityHistoryId;

											var historyList = changedFields.Select(field =>
												new EntityFieldHistoryChangeDto
												{
													EntityHistoryId = entityHistoryId,
													Module = "Attendees",
													SubModule = "Attendee",
													EntityAffected = $"{oldAttendee.FirstName} {oldAttendee.LastName}",
													User = userId,
													FieldName = field,
													OldValue = GeneralFunctions.GetOldValue(oldAttendee, field),
													NewValue = GeneralFunctions.GetNewValue(meetingUser, field)
												}).ToList();

											await _historyService.InsertEntityFieldHistoryChanges(historyList);
										}
									}
								}
								catch (Exception ex)
								{
									Log.Information("User Id {DateTime.UtcNow}, userId :{userId}, Excaption in Activity log", DateTime.UtcNow, userId, ex);
								}
							}



						}
					}
				}	

				return (listMeetingUser, errorMessage);
			}
			catch
			{
				errorMessage = "The data provided seems invalid, Please check.";
				return (listMeetingUser, errorMessage);
			}
		}

		/// <summary>
		/// Is Header Valid
		/// </summary>
		/// <param name="workSheet"></param>
		/// <returns></returns>
		private static bool IsHeaderValid(ExcelWorksheet workSheet)
		{
			var excelHeaders = new List<string>();

			for (int col = 1; col <= workSheet.Dimension.Columns; col++)
			{
				var header = Convert.ToString(workSheet.Cells[1, col].Value)?
					.Trim()
					.Replace(" ", "")
					.ToLower();

				if (!string.IsNullOrWhiteSpace(header))
					excelHeaders.Add(header);
			}

			foreach (var required in RequiredColumns)
			{
				var normalizedRequired = required
					.Trim()
					.Replace(" ", "")
					.ToLower();

				if (!excelHeaders.Contains(normalizedRequired))
					return false;
			}

			return true;
		}


		/// <summary>
		/// Validate Name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private static bool ValidateName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return false;

			return Regex.IsMatch(name, "^[a-zA-Z0-9 .,_]*$");
		}

		/// <summary>
		/// Validate Email
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		private static bool ValidateEmail(string email)
		{
			if (string.IsNullOrEmpty(email))
				return true;
			// Use a regular expression for email validation
			string emailPattern = @"^\S+@\S+\.\S+$";
			return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Validate Phone Number
		/// </summary>
		/// <param name="phoneNumber"></param>
		/// <returns></returns>
		private static bool ValidatePhoneNumber(string phoneNumber)
		{
			// Check if the phone number is null or empty
			if (string.IsNullOrEmpty(phoneNumber))
				return false; // Null ya empty number invalid hai

			// Ensure the phone number starts with a '+' followed by country code and is up to 20 characters long
			if (!phoneNumber.StartsWith("+") || phoneNumber.Length > 20)
				return false;

			// Regular expression for numeric validation with country code
			string numericPattern = @"^\+[1-9]{1}[0-9]{1,3}[0-9]{8,14}$";
			return Regex.IsMatch(phoneNumber, numericPattern);
		}


		/// <summary>
		/// Get All Meetings User Details
		/// </summary>
		/// <param name="meetingID"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUserDetails>> GetAllMeetingsUserDetails(int meetingID)
		{
			return await meetingUserRepository.GetAllMeetingsUserDetails(meetingID);
		}

		/// <summary>
		/// Get Attendees 
		/// </summary>
		/// <param name="meetingID"></param>
		/// <param name="searchValue"></param>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUserDetails>> GetAllMeetingsUserDetailsPaged(int meetingID, string searchValue, int start, int length, Dictionary<string, string> filters, string sortColumn, string sortDirection)
		{
			return await meetingUserRepository.GetAllMeetingsUserDetailsPaged(meetingID, searchValue, start, length, filters, sortColumn, sortDirection);
		}

		/// <summary>
		/// Get All Meetings User Details
		/// </summary>
		/// <param name="meetingID"></param>
		/// <returns></returns>
		public async Task<Response<SaveAlertSettingsRequest>> GetAllMeetingNotificationDetails(int meetingID)

		{
			return await meetingUserRepository.GetNotificationAlertSettings(meetingID);
		}



		public async Task<Response<SaveAlertSettingsRequest>> GetAllAttendeeNotificationDetails(int meetingID)
		{
			return await meetingUserRepository.GetAttendeeNotificationAlertSettings(meetingID);
		}

		public async Task<Response<SaveAlertSettingsRequest>> GetAlertNotificationByattendeeId(int ID)
		{
			return await meetingUserRepository.GetAlertNotificationAttendeeID(ID);
		}


		public async Task<Response<SaveAlertSettingsRequest>> GetAlertNotificationAlertSettings()
		{
			return await meetingUserRepository.GetAllNotificationAlertSettings();
		}


		/// <summary>
		/// Get Meetings User Details
		/// </summary>
		/// <param name="meetingID"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUserDetails>> GetMeetingsUserDetails(int meetingID)
		{
			return await meetingUserRepository.GetMeetingsUserDetails(meetingID);
		}

		/// <summary>
		/// Get All AllArchive Meetings User Details
		/// </summary>
		/// <param name="meetingID"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUserDetails>> GetAllArchiveMeetingsUserDetails(int meetingID)
		{
			return await meetingUserRepository.GetAllArchiveMeetingsUserDetails(meetingID);
		}


		/// <summary>
		/// Get All  Meetings User Details of FlightDelay
		/// </summary>
		/// <param name="meetingID"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUserDetails>> GetAllMeetingsUserDetailsFlightDelay(int meetingID)
		{
			return await meetingUserRepository.GetAllMeetingsUserDetailsFlightDelay(meetingID);
		}

		/// <summary>
		/// GetMeeting User By Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> GetMeetingUserById(int id)

		{
			return await this.meetingUserRepository.GetAsync(id);
		}

		/// <summary>
		/// Update Meeting User Data
		/// </summary>
		/// <param name="meetingUser"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> UpdateMeetingUserData(MeetingUser meetingUser)
		{
			string id = meetingUser.Id.ToString();
			var checkExist = await CheckMeetingUser(meetingUser, id);
			if (checkExist.Data.Count > 1)
			{
				return new Response<MeetingUser>
				{
					Message = Constants.DuplicateRecord,
					Status = HttpStatusCode.NotAcceptable
				};
			}
			return await UpdateRecords(meetingUser, id);
		}

		/// <summary>
		/// Delete Meeting User Data
		/// </summary>
		/// <param name="meetingUser"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> DeleteMeetingUserData(MeetingUser meetingUser)
		{

#pragma warning disable CS8604 // Possible null reference argument.
			var updateFields = new Dictionary<string, object> {
															{ "IsDeleted", meetingUser.IsDeleted },
															 { "IsActive", meetingUser.IsActive },
															{ "ModifiedBy", meetingUser.ModifiedBy },
															{ "ModifiedDate", DateTime.UtcNow },
														 };
#pragma warning restore CS8604 // Possible null reference argument.
			return await meetingUserRepository.PartialUpdate(updateFields, "Id", meetingUser).ConfigureAwait(false);
		}

		/// <summary>
		/// Get All Meeting User Data
		/// </summary>
		/// <param name="commaSeparatedIds"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> GetAllMeetingUserData(string commaSeparatedIds)
		{
			return await meetingUserRepository.GetAllDataAsync(commaSeparatedIds).ConfigureAwait(false);
		}

		/// <summary>
		/// Get All Meeting Id With User Data
		/// </summary>
		/// <param name="commaSeparatedIds"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> GetAllMeetingIdWithUserData(string commaSeparatedIds, bool MeetingFlag)
		{
			return await meetingUserRepository.GetAllMeetingIdWithUserData(commaSeparatedIds, MeetingFlag).ConfigureAwait(false);
		}

		/// <summary>
		/// Send Excaption Email
		/// </summary>
		/// <param name="htmlFilePath"></param>
		/// <param name="logoURL"></param>
		/// <param name="exception"></param>
		/// <param name="FunctionName"></param>
		/// <returns></returns>
		public async Task ExceptionSendMail(string htmlFilePath, string logoURL, Exception exception, string FunctionName)
		{
			try
			{
				var fullName = $"Exception";
				var plainTextContent = $"The flight given some Error : <br><br> Function Name: {FunctionName} <br> Excaption StackTrace: {exception.StackTrace}<br>Excaption Message: {exception.Message}";

				var htmlContent = await System.IO.File.ReadAllTextAsync(htmlFilePath);
				htmlContent = htmlContent.Replace("{{logoURL}}", logoURL).Replace("{{messageContent}}", plainTextContent).Replace("{{currentYear}}", DateTime.Now.Year.ToString());

				var emailDetails = new EmailDetails
				{
					APIKey = _configuration["EmailSenderOptions:apikey"],
					FromMailNew = _configuration["EmailSenderOptions:FromMailNew"],
					EmailFromName = _configuration["EmailSenderOptions:EmailFromName"],
					EmailID = _configuration["EmailSenderOptions:ExceptionEmailID"],
					FullName = fullName,
					Subject = Constants.FlightStatusAlert,
					MessageBody = htmlContent
				};

				await _emailService.SendEmailDetails(emailDetails);
			}
			catch (Exception ex)
			{
				#region Serilog
				Log.Information(string.Format("ExceptionSendMail running on {0} , Excaption{1}", DateTime.UtcNow, ex));
				#endregion
				await meetingUserRepository.LogException(ex, "ExceptionSendMail");
			}
		}

		/// <summary>
		/// Get Propertie Name
		/// </summary>
		/// <param name="propertyColumnName"></param>
		/// <returns></returns>
		private static string GetPropertieName(string propertyColumnName)
		{
			string propertyName = propertyColumnName;
			if (propertyName == "FlightNumber")
				propertyName = "DepartureFlightNumber";
			else if (propertyName == "FlightLabel")
				propertyName = "FlightType";
			return propertyName;
		}

		/// <summary>
		/// Check Meeting User
		/// </summary>
		/// <param name="meetingUser"></param>
		/// <returns></returns>	
		private async Task<Response<MeetingUser>> CheckMeetingUser(MeetingUser meetingUser, string ID)
		{
			var originalMeetingUser = await meetingUserRepository.CheckMeetingUser(meetingUser, ID).ConfigureAwait(false);
			if (originalMeetingUser == null || originalMeetingUser.Data.Count == 0)
			{
				await SetCarrierNameIfNeeded(meetingUser);
				return originalMeetingUser;
			}

			var originalUser = originalMeetingUser.Data[0];
			bool isDepartureFlightChanged = (originalUser.CarrierCode ?? "") != (meetingUser.CarrierCode ?? "");
			if (!string.IsNullOrWhiteSpace(meetingUser.CarrierCode) || string.IsNullOrWhiteSpace(originalUser.CarrierName))
			{
				await SetCarrierNameIfNeeded(meetingUser);
			}
			if (!HasMeetingUserChanged(originalUser, meetingUser))
			{
				meetingUser.state = originalUser.state;
				meetingUser.Status = originalUser.Status;
			}

			return originalMeetingUser;
		}


		/// <summary>
		/// Bulk Records Update
		/// </summary>
		/// <param name="meetingUser"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> UpdateRecords(MeetingUser meetingUser, string ID)
		{
		
			return await meetingUserRepository.UpdateRecords(meetingUser, ID).ConfigureAwait(false);
		}

		/// <summary>
		/// Bulk Send Alert Mail
		/// </summary>
		/// <param name="htmlFilePath"></param>
		/// <param name="logoURL"></param>
		/// <param name="meetingUser"></param>
		/// <returns></returns>
		public async Task BulkSendAlertMail(string htmlFilePath, string logoURL, List<MeetingUser> listMeetingUser)
		{
			string APIKey = _configuration["EmailSenderOptions:apikey"];
			// Create a list to hold SendGridMessage objects
			List<SendGridMessage> messages = new List<SendGridMessage>();
			string fromMailNew = _configuration["EmailSenderOptions:FromMailNew"];
			string emailFromName = _configuration["EmailSenderOptions:EmailFromName"];
			string subject = Constants.FlightStatusAlert;

			var sendMeetingUserData = listMeetingUser.Where(x => !string.IsNullOrEmpty(x.DepartureFlightNumber) && !string.IsNullOrEmpty(x.EmailId) && !string.IsNullOrEmpty(x.CarrierCode)).ToList();
			if (sendMeetingUserData.Count == 0)
				return;

			foreach (var meetingUser in sendMeetingUserData)
			{
				var client = new HttpClient();
				client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
				client.DefaultRequestHeaders.Add("Subscription-Key", _configuration["OAGApiSettings:SubscriptionKey"]);

				var baseUrl = _configuration["OAGApiSettings:BaseUrl"];
				var uri = $"{baseUrl}/?DepartureDateTime={meetingUser.DepartureDateTime:yyyy-MM-dd}&CarrierCode={meetingUser.CarrierCode}&FlightNumber={meetingUser.DepartureFlightNumber}&Content={Constants.Content}&CodeType={meetingUser.CodeType}&version={Constants.Version}";

				HttpResponseMessage response = await client.GetAsync(uri);
				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

					if (apiResponse?.Data != null && apiResponse?.Paging.TotalCount != 0)
					{
						foreach (var flight in apiResponse.Data)
						{
							string[] EmailID = meetingUser.EmailId.Split(',');
							foreach (var Email in EmailID)
							{
								DateTime dt = DateTime.Parse(flight.StatusDetails[0].Departure.estimatedTime.outGate.local);
								string formattedTime = dt.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);
								DateTime dateTime = DateTime.Parse(flight.StatusDetails[0].Arrival.actualtime.onground.local);
								string ArrivalTime = dateTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);

								var fullName = $"{meetingUser.FirstName} {meetingUser.LastName}";
								var plainTextContent = $"The following flight has an update: <br><br> Attendee: {fullName} <br><br> Flight Status: {flight.StatusDetails[0].Departure.estimatedTime.outGateTimeliness}<br>Delayed Time: {flight.StatusDetails[0].Departure.estimatedTime.outGateVariation} <br>Flight Number: {flight.FlightNumber}<br><br>Depature Time: {formattedTime} Local Time <br> Departure Airport: {flight.Departure.Airport.Iata},{flight.Departure.Airport.Icao} <br> Arrival Time: {ArrivalTime}  Local Time <br> Arrival Airport: {flight.Arrival.Airport.Iata}, {flight.Arrival.Airport.Icao}<br>";

								var htmlContent = await System.IO.File.ReadAllTextAsync(htmlFilePath);
								htmlContent = htmlContent.Replace("{{logoURL}}", logoURL).Replace("{{messageContent}}", plainTextContent).Replace("{{currentYear}}", DateTime.Now.Year.ToString());

								var from = new EmailAddress(fromMailNew, emailFromName);
								var to = new EmailAddress(Email, fullName);
								var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
								messages.Add(msg);
							}
						}
					}
				}
			}

			if (messages.Count > 0)
				await _emailService.BulkSendEmailDetails(messages, APIKey);

		}

		private async Task<List<FlightStatusDetails>> GetFlightStatus()
		{
			#region Serilog
			Log.Information(string.Format("GetFlightStatus method running on {0}", DateTime.UtcNow));
			#endregion
			var EventHubConnectionString = _configuration["OAGApiSettings:EventHubConnectionString"];
			var EventHubName = _configuration["OAGApiSettings:EventHubName"];
			string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
			List<FlightStatusDetails> lstFlightStatus = new List<FlightStatusDetails>();
			try
			{
				await using (var consumerClient = new EventHubConsumerClient(consumerGroup, EventHubConnectionString, EventHubName))
				{
					ReadEventOptions readOptions = new ReadEventOptions();
					readOptions.MaximumWaitTime = TimeSpan.FromSeconds(5);
					await foreach (PartitionEvent partitionEvent in consumerClient.ReadEventsAsync(readOptions))
					{
						if (partitionEvent.Data != null)
						{
							string eventBody = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
							// dynamic eventData = JsonConvert.DeserializeObject(eventBody);
							var eventData = JsonConvert.DeserializeObject<EventHubResponse>(eventBody);
							if (eventData != null && eventData.State != null && eventData.MessageId != null)
							{
								lstFlightStatus.Add(EventHubResponseMapper.MapToUserFlightStatus(eventData));
							}
						}
						else
							break;
					}
					return lstFlightStatus;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Bulk Send Alert Mail with event hub
		/// </summary>
		/// <param name="htmlFilePath"></param>
		/// <param name="logoURL"></param>
		/// <param name="meetingUser"></param>
		/// <returns></returns>

		public async Task BulkSendAlertMailEventHub(string htmlFilePath, string logoURL, string Trigger, string meetingId, bool MeetingFlag)
		{
			#region Serilog

			Log.Information($"BulkSendAlertMailEventHub method running on {DateTime.UtcNow}");
			#endregion

			try
			{
				string APIKey = _configuration["EmailSenderOptions:apikey"];
				List<SendGridMessage> messages = new List<SendGridMessage>();
				string fromMailNew = _configuration["EmailSenderOptions:FromMailNew"];
				string emailFromName = _configuration["EmailSenderOptions:EmailFromName"];

				var lstEventHubData = await GetFlightStatus();

				List<FlightStatusDetails> userFlightStatuses = new List<FlightStatusDetails>();

				#region Set All Meeting 
				List<MeetingUser> sendMeetingUserData = new List<MeetingUser>();
				if (string.IsNullOrWhiteSpace(meetingId))
				{
					int Hours = int.Parse(_configuration["WebJobSetting:Hours48"]);
					var listMeetingUser = await GetAllMeetingIdWithUserDataWebJob(Hours).ConfigureAwait(false);
					sendMeetingUserData = listMeetingUser.Data.Where(x => !string.IsNullOrEmpty(x.AlertId)).ToList();
				}
				else
				{
					var listMeetingUser = await GetAllMeetingIdWithUserData(meetingId, MeetingFlag).ConfigureAwait(false);
					sendMeetingUserData = listMeetingUser.Data.Where(x => !string.IsNullOrEmpty(x.AlertId)).ToList();
				}
				if (sendMeetingUserData == null || sendMeetingUserData.Count == 0)
					return;
				var allMeetingID = sendMeetingUserData.Select(x => x.MeetingID).Distinct().ToList();
				string commaSeparatedMeetingIds = string.Join(",", allMeetingID);
				var allMeetings = await meetingsRepository.GetAllMeetingDetailsId(commaSeparatedMeetingIds);

				var Alertsetting = await GetAlertNotificationAlertSettings();

				#endregion

				foreach (var meetingUser in sendMeetingUserData)
				{
					#region Serilog

					Log.Information($"BulkSendAlertMailEventHub method running on {DateTime.UtcNow} Meeting user details {meetingUser.FirstName} {meetingUser.LastName}");
					#endregion

					var meetingData = allMeetings.Data.Where(x => x.Id == Convert.ToInt32(meetingUser.MeetingID)).FirstOrDefault();
					var effectiveAlert = Alertsetting.Data.FirstOrDefault(x => x.AttendeeId == meetingUser.Id);

					if (effectiveAlert == null)
					{
						effectiveAlert = Alertsetting.Data.FirstOrDefault(x =>
							x.EntityType?.ToLower() == meetingUser.AttendeeType.ToLower());
					}

					if (effectiveAlert == null)
					{
						effectiveAlert = Alertsetting.Data.FirstOrDefault(x => x.MeetingId == meetingUser.MeetingID);
					}
					List<string> selectedEmailStates;
					List<string> selectedEmailStatuses;
					List<string> selectedSmsStates;
					List<string> selectedSmsStatuses;
					bool sendEmail;
					bool sendSms;

					if (effectiveAlert == null)
					{
						// Default values
						selectedEmailStates = new List<string> { AlertStateEnum.Canceled.ToString().ToLower() };
						selectedEmailStatuses = new List<string> { AlertStatusEnum.Delayed.ToString().ToLower() };
						selectedSmsStates = new List<string> { AlertStateEnum.Canceled.ToString().ToLower() };
						selectedSmsStatuses = new List<string> { AlertStatusEnum.Delayed.ToString().ToLower() };
						sendEmail = true;
						sendSms = true;
					}
					else
					{
						selectedEmailStates = effectiveAlert.EmailStates?.Select(e => e.ToString().ToLower()).ToList() ?? new();
						selectedEmailStatuses = effectiveAlert.EmailStatuses?.Select(e => e.ToString().ToLower()).ToList() ?? new();
						selectedSmsStates = effectiveAlert.SmsStates?.Select(e => e.ToString().ToLower()).ToList() ?? new();
						selectedSmsStatuses = effectiveAlert.SmsStatuses?.Select(e => e.ToString().ToLower()).ToList() ?? new();
						sendEmail = effectiveAlert.EmailAlert;
						sendSms = effectiveAlert.SmsAlert;
					}

					if (sendEmail == true || sendSms == true)
					{
						var eventHub = lstEventHubData
						.Where(f => f.AlertId == meetingUser.AlertId && DateTime.TryParse(f.DepartureTimesScheduledLocal, out var depTime) &&
		Math.Abs((depTime - meetingUser.DepartureDateTime).TotalMinutes) <= 5)
						.OrderByDescending(a => a.MessageTimestamp)
						.FirstOrDefault();

						TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

						DateTime serverLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);


						if (eventHub != null)
						{
							eventHub.MeetingUserID = meetingUser.Id;
							eventHub.CreateDate = DateTime.Now;
							eventHub.IsDeleted = false;
							eventHub.IsActive = true;

                            var Flightstatus = !string.IsNullOrWhiteSpace(eventHub.DepartureTimesEstimatedOutGateTimeliness)
									? eventHub.DepartureTimesEstimatedOutGateTimeliness
									: meetingUser.Status;
                            if (!string.IsNullOrEmpty(Flightstatus) && Flightstatus.ToLower() == Constants.Delayed.ToLower())
                            {
								Flightstatus = eventHub.DepartureTimesEstimatedOutGateTimeliness + " by " + eventHub.DepartureTimesEstimatedOutGateVariation;
							}

                            if (!string.IsNullOrEmpty(Flightstatus) && Flightstatus.ToLower() == Constants.Early.ToLower())
                            {
								eventHub.DepartureTimesEstimatedOutGateVariation = TimeSpan.Zero;

							}
							var MeeetingState = meetingUser.state?.ToLower() ?? string.Empty;
							var MeeetingStatus = meetingUser.Status?.ToLower() ?? string.Empty;
							var FlightState = !string.IsNullOrWhiteSpace(eventHub.State) ? eventHub.State : meetingUser.state;
							var EmailSend = false;
							var SMSSend = false;
							var plainTextContent = string.Empty;
							var smsContent = string.Empty;
							string EmailtriggerDateTimeUtc = string.Empty;
							string SmstriggerDateTimeUtc = string.Empty;

							#region Serilog

							Log.Information($"BulkSendAlertMailEventHub method running on {DateTime.UtcNow} EventHub Status : {Flightstatus} EventHub State: {eventHub.State} Meeting Status : {MeeetingStatus} Meeting State {MeeetingState}");
							#endregion

							if (MeeetingState != Constants.Landed.ToLower() && MeeetingState != Constants.InGate.ToLower())
							{
								if (Flightstatus?.ToLower() != MeeetingStatus || FlightState?.ToLower() != MeeetingState)
								{
									// Email condition based on selected states and statuses
									if (sendEmail)
									{

										bool shouldSendEmail = false;

										// Check if Flightstatus or FlightState is in selected lists
										var EventHubFlightstatus = Flightstatus?.Trim().ToLower();
                                        // Clean the status - remove " by <duration>"
                                        if (EventHubFlightstatus != null && EventHubFlightstatus.Contains(" by "))
                                            EventHubFlightstatus = EventHubFlightstatus.Split(new[] { " by " }, StringSplitOptions.None)[0].Trim();

                                        var EventHubFlightState = FlightState?.Trim().ToLower();

										// Check if either Flightstatus or FlightState is in the respective selected lists
										if ((MeeetingStatus != Flightstatus?.ToLower() && selectedEmailStatuses.Any(status => status.Trim().ToLower() == EventHubFlightstatus)) ||
											(MeeetingState != FlightState?.ToLower() && selectedEmailStates.Any(state => state.Trim().ToLower() == EventHubFlightState)))
										{
											#region Serilog

											Log.Information($"BulkSendAlertMailEventHub method running on {DateTime.UtcNow} EventHub Status : {Flightstatus} EventHub State: {eventHub.State} Meeting Status : {MeeetingStatus} Meeting State {MeeetingState} Email send condition True");
											#endregion

											shouldSendEmail = true;
										}
										if (MeeetingStatus == Constants.Delayed.ToLower() && EventHubFlightstatus?.ToLower() != MeeetingStatus)
										{
											shouldSendEmail = true;
										}



										if (shouldSendEmail)
										{
											#region Serilog
											Log.Information($"BulkSendAlertMailEventHub method running on {DateTime.UtcNow} SendEmail Function");
											#endregion

											string[] EmailID = meetingUser.EmailId.Split(',');
											foreach (var Email in EmailID)
											{
												string subject = $"Flight Status Alerts - {Flightstatus}- {FlightState} - {meetingUser.FirstName} {meetingUser.LastName} - {meetingUser.DepartureFlightNumber} ";
												//DateTime dt = DateTime.Parse(eventHub.DepartureTimesScheduledLocal);
												//string formattedTime = dt.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);
												DateTime dt = DateTime.Parse(eventHub.DepartureTimesScheduledLocal);
												DateTime adjustedDepartureTime = dt.Add(eventHub.DepartureTimesEstimatedOutGateVariation);  // Add TimeSpan
												string formattedTime = adjustedDepartureTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);

												//DateTime dateTime = DateTime.Parse(eventHub.ArrivalTimesScheduledLocal);
												//string ArrivalTime = dateTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);

												DateTime dateTime = DateTime.Parse(eventHub.ArrivalTimesScheduledLocal);
												DateTime adjustedArrivalTime = dateTime.Add(eventHub.DepartureTimesEstimatedOutGateVariation);  // Add TimeSpan (if applicable)
												string ArrivalTime = adjustedArrivalTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);


												string LastSyncDateTimeUtc = DateTime.UtcNow.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);
												string webjobDateTimeUtc = DateTime.UtcNow.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);
												string EventHubEpochtime = eventHub.MessageTimestamp;

												var fullName = $"{meetingUser.FirstName} {meetingUser.LastName}";
												plainTextContent = $"The following flight has an update: <br><br> Attendee: {fullName}<br><br> Meeting Name: {meetingData.MeetingName} <br><br>Flight State: {FlightState}<br> Flight Status: {Flightstatus}<br>Delayed Time: {eventHub.DepartureTimesEstimatedOutGateVariation} <br>Flight Number: {eventHub.FlightNumber}<br>Airline: {meetingUser.CarrierName}<br><br>Depature Time: {formattedTime} Local Time <br> Departure Airport: {eventHub.DepartureAirportIata},{eventHub.DepartureAirportIcao} <br> Arrival Time: {ArrivalTime}  Local Time <br> Arrival Airport: {eventHub.ArrivalAirportIata}, {eventHub.ArrivalAirportIcao}<br> <br> EventHub Epochtime: {EventHubEpochtime} <br> LastSyncDateTimeUtc: {LastSyncDateTimeUtc} <br> Webjob DateTimeUtc: {webjobDateTimeUtc} <br> ";

												var htmlContent = await System.IO.File.ReadAllTextAsync(htmlFilePath);
												htmlContent = htmlContent.Replace("{{logoURL}}", logoURL).Replace("{{messageContent}}", plainTextContent).Replace("{{currentYear}}", DateTime.Now.Year.ToString());


												var emailDetails = new EmailDetails
												{
													APIKey = _configuration["EmailSenderOptions:apikey"],
													FromMailNew = fromMailNew,
													EmailFromName = "𝘱-value",
													EmailID = Email,
													FullName = fullName,
													Subject = subject,
													MessageBody = htmlContent
												};

												bool result = await _emailService.SendEmailAsync(emailDetails);
												EmailSend = result;
												EmailtriggerDateTimeUtc = DateTime.UtcNow.ToString("MM/dd/yyyy - hh:mm:ss tt", CultureInfo.InvariantCulture);

											}

										}
									}

									// SMS condition based on selected states and statuses
									if (sendSms)
									{
										bool shouldSendSms = false;
										// Check if Flightstatus or FlightState is in selected lists
										var EventHubFlightstatus = Flightstatus?.Trim().ToLower();
                                        // Clean the status - remove " by <duration>"
                                        if (EventHubFlightstatus != null && EventHubFlightstatus.Contains(" by "))
                                            EventHubFlightstatus = EventHubFlightstatus.Split(new[] { " by " }, StringSplitOptions.None)[0].Trim();
                                        var EventHubFlightState = FlightState?.Trim().ToLower();

										// Check if either Flightstatus or FlightState is in the respective selected lists
										if ((MeeetingStatus != Flightstatus?.ToLower() && selectedSmsStatuses.Any(status => status.Trim().ToLower() == EventHubFlightstatus)) ||
											(MeeetingState != FlightState?.ToLower() && selectedSmsStates.Any(state => state.Trim().ToLower() == EventHubFlightState)))
										{
											#region Serilog

											Log.Information($"BulkSendAlertMailEventHub method running on {DateTime.UtcNow} EventHub Status : {Flightstatus} EventHub State: {eventHub.State} Meeting Status : {MeeetingStatus} Meeting State {MeeetingState} Sms send condition True");
											#endregion

											shouldSendSms = true;
										}
										if (MeeetingStatus == Constants.Delayed.ToLower() && EventHubFlightstatus?.ToLower() != MeeetingStatus)
										{
											shouldSendSms = true;
										}


										if (shouldSendSms)
										{

											string[] PhoneNumbers = meetingUser.PhoneNumber.Trim().Split(',');
											foreach (var PhoneNumber in PhoneNumbers)
											{


												#region Serilog

												Log.Information($"BulkSendAlertMailEventHub method running on {DateTime.UtcNow} Send Sms Function ");
												#endregion

												// Formatting the time for SMS
												//DateTime dt = DateTime.Parse(eventHub.DepartureTimesScheduledLocal);
												//string formattedTime = dt.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);
												DateTime dt = DateTime.Parse(eventHub.DepartureTimesScheduledLocal);
												DateTime adjustedDepartureTime = dt.Add(eventHub.DepartureTimesEstimatedOutGateVariation);  // Add TimeSpan
												string formattedTime = adjustedDepartureTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);

												//                                 DateTime dateTime = DateTime.Parse(eventHub.ArrivalTimesScheduledLocal);
												//string ArrivalTime = dateTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);

												DateTime dateTime = DateTime.Parse(eventHub.ArrivalTimesScheduledLocal);
												DateTime adjustedArrivalTime = dateTime.Add(eventHub.DepartureTimesEstimatedOutGateVariation);  // Add TimeSpan (if applicable)
												string ArrivalTime = adjustedArrivalTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);



												smsContent = $"Flight update for {meetingUser.FirstName} {meetingUser.LastName}\n" +
																 $"Meeting Name: {meetingData.MeetingName}\n" +
																 $"Status: {Flightstatus}\n" +
																 $"State: {FlightState}\n" +
																 $"Airline: {meetingUser.CarrierName}\n" +
																 $"Flight Number: {eventHub.FlightNumber}\n" +
																 $"Departure Time: {formattedTime}\n" +
																 $"Arrival Time: {ArrivalTime}\n" +
																 $"Regards, Team 𝘱-value";

												if (!string.IsNullOrEmpty(meetingUser.PhoneNumber))
												{
													_smsServices.SendSms(meetingUser.PhoneNumber, smsContent);
													SMSSend = true;
													SmstriggerDateTimeUtc = DateTime.UtcNow.ToString("MM/dd/yyyy - hh:mm:ss tt", CultureInfo.InvariantCulture);
												}

											}
										}
									}

									if (meetingUser.EmailSend == true)
									{
										EmailSend = true;
									}
									if (meetingUser.SmsSend == true)
									{
										SMSSend = true;
									}

									// Update the last sync date
									await UpdateLastsyncDateAsync(meetingUser, serverLocalTime.ToString(), Flightstatus, FlightState, EmailSend, SMSSend, eventHub.MessageTimestamp, eventHub.MessageId, EmailtriggerDateTimeUtc, SmstriggerDateTimeUtc).ConfigureAwait(false);
									await meetingsRepository.NotificationLog("BulkSendAlertMailEventHub", MeeetingStatus, Flightstatus, MeeetingState, FlightState, meetingUser.DepartureFlightNumber, eventHub.DepartureTimesScheduledLocal, eventHub.ArrivalTimesScheduledLocal, meetingUser.CarrierCode, Trigger, plainTextContent, smsContent, EmailSend, SMSSend).ConfigureAwait(false);
									string EventHubstring = JsonConvert.SerializeObject(eventHub,
										new JsonSerializerSettings()
										{
											ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
										});
									var ObjHub = JsonConvert.DeserializeObject<FlightStatusDetails>(EventHubstring);
									userFlightStatuses.Add(ObjHub);
								}
							}
							else
							{
								if (eventHub != null)
								{
									await UpdateLastsyncDateAsync(meetingUser, serverLocalTime.ToString("dd/MM/yyyy, hh:mm:ss tt", CultureInfo.InvariantCulture), Flightstatus, FlightState, false, false, eventHub.MessageTimestamp, eventHub.MessageId, string.Empty, string.Empty).ConfigureAwait(false);
								}
								else
								{
									await UpdateLastsyncDateAsync(meetingUser, serverLocalTime.ToString("dd/MM/yyyy, hh:mm:ss tt", CultureInfo.InvariantCulture), Constants.NoRecord, "", false, false, "", "", string.Empty, string.Empty).ConfigureAwait(false);
								}
							}
						}
						List<FlightStatusDetails> existingFlightStatuses = this.userFlightStatusRepository.GetMeetingId(userFlightStatuses).ToList();
						List<FlightStatusDetails> uniqueFlightStatuses = userFlightStatuses.Where(u => !existingFlightStatuses.Any(l => l.MessageId == u.MessageId)).ToList();
						if (uniqueFlightStatuses.Count > 0)
							await this.userFlightStatusRepository.BulkInsert(uniqueFlightStatuses);
					}

					//if (messages.Count > 0)
					//	await _emailService.BulkSendEmailDetails(messages, APIKey);
				}
			}
			catch (Exception ex)
			{

				#region Serilog
				Log.Information($"BulkSendAlertMailEventHub running on {DateTime.UtcNow}, Exception: {ex}");
				#endregion
				await meetingUserRepository.LogException(ex, "BulkSendAlertMailEventHub");
				await ExceptionSendMail(htmlFilePath, logoURL, ex, "BulkSendAlertMailEventHub");
			}
		}

		/// <summary>
		/// Create Alert when Alert ID is null
		/// </summary>
		/// <returns></returns>

		public async Task<Response<MeetingUser>> createAlertAsync()
		{
			var res = await meetingUserRepository.createalert();

			return res;
		}

		public async Task<Response<MeetingUser>> createAlertAsync(List<int> MeetingId)
		{
			var res = await meetingUserRepository.createalert(MeetingId);

			return res;
		}
		/// <summary>
		/// Update Meeting User Table Flight Info Column 
		/// </summary>
		/// <param name="meetingUser"></param>
		/// <param name="DateTime"></param>
		/// <param name="status"></param>
		/// <param name="state"></param>
		/// <param name="EmailSend"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> UpdateLastsyncDateAsync(MeetingUser meetingUser, string serverLocalTime, string status, string state, Boolean EmailSend, Boolean SMSSend, string MessageTimestamp, string MessageId, string EmailtriggerDateTimeUtc, string SmstriggerDateTimeUtc)
		{
			var updateFields = new Dictionary<string, object> {
															{ "LastSyncDateTime", serverLocalTime},
															{ "LastSyncDateTimeUtc", DateTime.UtcNow.ToString("dd/MM/yyyy, hh:mm:ss tt", CultureInfo.InvariantCulture)},
															{ "MessageTimestamp", MessageTimestamp},
															{ "MessageId", MessageId},
															{ "Status", status},
															{ "State", state},
															{ "EmailSend", EmailSend},
															{ "SmsSend", SMSSend},
															{ "EmailtriggerDateTimeUtc", EmailtriggerDateTimeUtc},
															{ "SmstriggerDateTimeUtc", SmstriggerDateTimeUtc},
														 };
			var res = await meetingUserRepository.PartialUpdate(updateFields, "Id", meetingUser).ConfigureAwait(false);

			return res;
		}
		/// <summary>
		/// Update Alert Id 
		/// </summary>
		/// <param name="meetingUser"></param>
		/// <param name="AlertID"></param>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> UpdateAlertAsync(MeetingUser meetingUser, string AlertID)
		{
			var updateFields = new Dictionary<string, object> {
															{ "AlertId", AlertID},
														 };
			var res = await meetingUserRepository.PartialUpdate(updateFields, "Id", meetingUser).ConfigureAwait(false);

			return res;
		}
		/// <summary>
		/// Get All Meeting in Web job 
		/// </summary>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> GetAllMeetingIdWithUserDataWebJob(int Hours)
		{
			return await meetingUserRepository.GetAllMeetingIdWithUserDataFasWebJob(Hours);

		}
		/// <summary>
		/// Get All Meeting in Web job for every 15 min
		/// </summary>
		/// <returns></returns>
		public async Task<Response<MeetingUser>> GetAllMeetingIdWithUserDataFasWebJob(int Hours)
		{
			return await meetingUserRepository.GetAllMeetingIdWithUserDataFasWebJob(Hours);

		}
		public async Task Exceptionsend(Exception ex, string FunctionName)
		{
			await meetingUserRepository.LogException(ex, FunctionName);
		}
		/// <summary>
		/// MoveDataintoArchiveTable from webjob 
		/// </summary>
		/// <returns></returns>
		public async Task MoveDataintoArchiveTable()
		{
			await meetingUserRepository.MoveDataintoArchiveTable();
			await meetingUserRepository.Mettingsdelete();
		}
		/// <summary>
		/// MoveDataintoArchiveTable form manually by User 
		/// </summary>
		/// <param name="MeetingID"></param>
		/// <returns></returns>
		public async Task MoveDataintoArchiveTable(int MeetingID)
		{
			await meetingUserRepository.MoveDataintoArchiveTable(MeetingID);
			await meetingUserRepository.Mettingsdelete();
		}

		/// <summary>
		/// GetallAlertsbyMeetingID
		/// </summary>
		/// <param name="MeetingID"></param>
		/// <returns></returns>
		public async Task<List<string>> GetallAlertsbyMeetingID(int MeetingID)
		{
			var alertIdsQueryable = await meetingUserRepository.GetAllAlertsbyMeetingID(MeetingID);

			var alertIds = alertIdsQueryable.ToList();

			return alertIds;
		}

		/// <summary>
		///  UpdateAlertsetting
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public async Task<bool> UpdateAlertsettingAsync(SaveAlertSettingsRequest request)
		{
			var res = await meetingUserRepository.SaveOrUpdateAlertSettings(request);
			return res;
		}
		/// <summary>
		/// save NotificationAlertsettingAsync
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public async Task<bool> NotificationAlertsettingAsync(SaveAlertSettingsRequest model, int userId)
		{
			var res = await meetingUserRepository.SaveOrUpdateNotificationTabAlertSettings(model, userId);
			return res;
		}

		/// <summary>
		/// MultipleNotificationAlertsetting
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public async Task<bool> MultipleNotificationAlertsettingAsync(SaveAlertSettingsRequest model, int userId)
		{
			var res = await meetingUserRepository.SaveOrUpdateMultipleNotificationTabAlertSettings(model, userId);
			return res;
		}

		/// <summary>
		/// Get old NotificationAlertsetting
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public async Task<Dictionary<int, AlertSettingOldValueDto>> oldNotificationAlertsettingAsync(SaveAlertSettingsRequest model)
		{
			var res = await meetingUserRepository.GetOldAlertSettingAsync(model);
			return res;
		}


		/// <summary>
		/// Get old NotificationAlertsetting Attendee Type
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public async Task<AlertSettingOldValueDto?> GetOldAlertSettingForAttendeeTypeAsync(SaveAlertSettingsRequest model)
		{
			var res = await meetingUserRepository.GetOldAlertSettingForAttendeeTypeAsync(model);
			return res;
		}


		/// <summary>
		///  MultipleNotificationAlertsettingForTypes
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public async Task<bool> MultipleNotificationAlertsettingForTypes(SaveAlertSettingsRequest model, int userId)
		{
			var res = await meetingUserRepository.SaveOrUpdateNotificationAlertSettingsForTypes(model, userId);
			return res;
		}

		/// <summary>
		/// Insert AlertSetting parent history
		/// </summary>
		public async Task<Response<int>> InsertAlertSettingAuditHistory(
			AlertSettingHistoryAuditDto dto)
		{
			var result =
				await meetingUserRepository.InsertAlertSettingAuditHistory(dto);

			return result;
		}

		/// <summary>
		/// Insert AlertSetting field history changes
		/// </summary>
		public async Task<bool> InsertAlertSettingFieldHistoryChanges(
			List<AlertSettingFieldHistoryChangeDto> list)
		{
			var result =
				await meetingUserRepository.InsertAlertSettingFieldHistoryChanges(list);

			return result;
		}

		/// <summary>
		/// HasMeetingUserChanged
		/// </summary>
		/// <param name="original"></param>
		/// <param name="updated"></param>
		/// <returns></returns>
		private bool HasMeetingUserChanged(MeetingUser original, MeetingUser updated)
		{
			bool isFlightNumberChanged = (original.DepartureFlightNumber ?? "") != (updated.DepartureFlightNumber ?? "");
			bool isDepartureDateTimeChanged = original.DepartureDateTime != updated.DepartureDateTime;
			bool isArrivalDateTimeChanged = original.ArrivalDateTime != updated.ArrivalDateTime;
			bool isOriginAirportChanged = (original.OriginAirport ?? "") != (updated.OriginAirport ?? "");
			bool isDestinationAirportChanged = (original.DestinationAirport ?? "") != (updated.DestinationAirport ?? "");

			return isFlightNumberChanged ||
				   isDepartureDateTimeChanged ||
				   isArrivalDateTimeChanged ||
				   isOriginAirportChanged ||
				   isDestinationAirportChanged;
		}

		/// <summary>
		/// Get Carriers Details
		/// </summary>
		/// <param name="carrierCode"></param>
		/// <returns></returns>
		public async Task<string?> GetCarriersDetails(string carrierCode)
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
			client.DefaultRequestHeaders.Add("Subscription-Key", _configuration["OAGApiSettings:SubscriptionKey"]);
			var _apiUrl = _configuration["OAGApiSettings:BaseUrlCarriers"];
			string url = string.Format(_apiUrl, carrierCode);
			try
			{
				var response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode();
				string responseContent = await response.Content.ReadAsStringAsync();
				JObject jsonResponse = JObject.Parse(responseContent);
				return jsonResponse["name"]?.ToString() ?? carrierCode;
			}
			catch (HttpRequestException ex)
			{
				return null;
			}
			catch (TaskCanceledException ex)
			{
				return null;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		/// <summary>
		/// SetCarrierNameIfNeeded
		/// </summary>
		/// <param name="meetingUser"></param>
		/// <returns></returns>
		private async Task SetCarrierNameIfNeeded(MeetingUser meetingUser)
		{
			if (!string.IsNullOrWhiteSpace(meetingUser.CarrierCode))
			{
				string Carriercode = meetingUser.CarrierCode.ToUpper();

				meetingUser.CarrierName = GeneralFunctions.GetCarrierName(Carriercode);
				if (string.IsNullOrWhiteSpace(meetingUser.CarrierName))
					meetingUser.CarrierName = meetingUser.CarrierCode;
			}
		}

		/// <summary>
		/// InsertEventHubUserLogAsync
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		public async Task<Response<int>> InsertEventHubUserLogAsync(EventHubUserLog logEntry)
		{
			return await meetingUserRepository.InsertEventHubUserLogAsync(logEntry);
		}

		/// <summary>
		/// GetAllMeetingUsersPaginated
		/// </summary>
		/// <returns></returns>
		public async Task<Response<MeetingUserDetails>> GetAllMeetingUsersPaginated(AttendeeSearchRequest request)
		{
			return await meetingUserRepository.GetAllMeetingUsersPaginated(request);
		}

		/// <summary>
		/// GetAllAttendeeNameType
		/// </summary>
		/// <returns></returns>
		public async Task<Response<AttendeeFilterData>> GetAllAttendeeNameType(int meetingID)
		{
			return await meetingUserRepository.GetAllAttendeeNameType(meetingID);
		}

		public async Task<Response<SaveAlertSettingsRequest>> GetAttendeeNotificationDetails(List<int> attendeeIds)
		{
			return await meetingUserRepository.GetAttendeeNotificationAlertSetting(attendeeIds);
		}

		public async Task<object> GetFilterOptions(int meetingId)
		{
			return await meetingUserRepository.GetFilterOptions(meetingId);
		}

		/// <summary>
		///  UpdateActivityMeetingsettingAsync
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public async Task UpdateActivityMeetingsettingAsync(ActivityMeetingDetails model)
		{
			 await meetingUserRepository.UpdateActivityMeetingSettingAsync(model);
		}

		/// <summary>
		/// UpdateEmailStatus
		/// </summary>
		/// <param name="status"></param>
		/// <param name="messageId"></param>
		/// <returns></returns>
		public async Task UpdateEmailStatus(string status, string messageId)
		{
			var updateFields = new Dictionary<string, object>
	{
		{ "EmailStatus", status }
	};

			var entity = new MeetingUser
			{
				EmailMessageId = messageId
			};

			await meetingUserRepository.PartialUpdate(updateFields, "EmailMessageId", entity);
		}

		/// <summary>
		/// UpdateSmsStatus
		/// </summary>
		/// <param name="status"></param>
		/// <param name="messageId"></param>
		/// <returns></returns>

		public async Task UpdateSmsStatus(string status, string messageId)
		{
			var updateFields = new Dictionary<string, object>
	{
		{ "SmsStatus", status }
	};

			var entity = new MeetingUser
			{
				SmsMessageId = messageId
			};

			await meetingUserRepository.PartialUpdate(updateFields, "SmsMessageId", entity);
		}
	}
}
