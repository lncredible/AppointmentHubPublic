using AppointmentManagementSystem.Data;
using AppointmentManagementSystem.Models;

namespace AppointmentManagementSystem.Services
{
    public class SmsService : ISmsService
    {
        private int _iDurationBeforeAppt = 48;
        private readonly AccountDbContext _oAccountDbContext;
        private readonly AppointmentDbContext _oAppointmentDbContext;
        private readonly string _sLogFilePath = @"SmsLog.txt";
        private readonly string _sSentSmsFilePath = @"SentSmsLog.txt";

        public SmsService(AccountDbContext oAccountDbContext, AppointmentDbContext oAppointmentDbContext)
        {
            _oAccountDbContext = oAccountDbContext;
            _oAppointmentDbContext = oAppointmentDbContext;
        }

        public void StoreAppointmentsDue()
        {
            IEnumerable<Appointment> ieAppointments = _oAppointmentDbContext.appointments.ToList();

            HashSet<int> hsProcessedAppointmentIDs = GetProcessedAppointmentIds();

            Console.WriteLine("ieAppointments: " + ieAppointments.Count());

            foreach (Appointment oAppointment in ieAppointments)
            {
                if (hsProcessedAppointmentIDs.Contains(oAppointment.AppointmentId))
                {
                    continue;
                }

                DateTime dtAppointment = new DateTime(
                oAppointment.AppointmentDate.Year,
                oAppointment.AppointmentDate.Month,
                oAppointment.AppointmentDate.Day,
                oAppointment.AppointmentTime.Hour,
                oAppointment.AppointmentTime.Minute,
                oAppointment.AppointmentTime.Second
);

                DateTime dtCurrentTime = DateTime.Now;
                TimeSpan tsDifference = dtAppointment - dtCurrentTime;
                double dbAbsoluteDifference = Math.Abs(tsDifference.TotalHours);
                Console.WriteLine("Time Difference: " + tsDifference.Hours.ToString());
                if (dbAbsoluteDifference <= _iDurationBeforeAppt)
                {
                    SendSms(CreateSms(oAppointment));
                    hsProcessedAppointmentIDs.Add(oAppointment.AppointmentId);
                    SaveProcessedAppointmentIds(hsProcessedAppointmentIDs);
                }
            }
        }

        public string CreateSms(Appointment oAppointment)
        {
            DateTime dtDate = oAppointment.AppointmentDate.Date;
            DateOnly doDate = new DateOnly(dtDate.Year, dtDate.Month, dtDate.Day);
            DateTime tsTime = oAppointment.AppointmentTime;
            string sSubject = oAppointment.AppointmentSubject;
            string sEmail = oAppointment.UserEmail;


            var vUser = _oAccountDbContext.Users.FirstOrDefault(u => u.Email == sEmail);

            if (vUser == null) return $"No user found with email: {sEmail}";

            string sFirstName = vUser.FirstName;
            string sLastName = vUser.LastName;
            string? sPhoneNumber = vUser.PhoneNumber;

            return $"Message sent to {sPhoneNumber} at {DateTime.Now}:\n" +
       $"Hi {sFirstName} {sLastName}, you have an appointment on {doDate} at {tsTime.ToString("HH:mm")} for a {sSubject}. See you soon!\n";
        }

        public void SendSms(string sMessage)
        {
            Console.WriteLine("SendSms function in the SmsService class is invoked\n\n" + sMessage);

            try
            {
                File.AppendAllText(_sLogFilePath, sMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        private HashSet<int> GetProcessedAppointmentIds()
        {
            if (File.Exists(_sSentSmsFilePath))
            {
                return new HashSet<int>(File.ReadAllLines(_sSentSmsFilePath).Select(int.Parse));
            }
            else
            {
                return new HashSet<int>();
            }
        }

        private void SaveProcessedAppointmentIds(HashSet<int> appointmentIds)
        {
            File.WriteAllLines(_sSentSmsFilePath, appointmentIds.Select(id => id.ToString()));
        }
    }
}