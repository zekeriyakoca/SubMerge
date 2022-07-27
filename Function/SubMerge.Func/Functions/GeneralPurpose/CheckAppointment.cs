using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace SubMerge.Func.GeneralPurpose
{
    public class CheckAppointment
    {
        private HttpClient client { get; }

        public CheckAppointment()
        {
            client = new HttpClient();
        }

        [FunctionName("CheckAppointment")]
        public async Task Run([TimerTrigger("0 */50 * * * *", RunOnStartup = true)] TimerInfo myTimer,
            [SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")] IAsyncCollector<SendGridMessage> messageCollector,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var response = await client.GetAsync(BuildUrl());
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            var cleanText = result.Replace(")]}',\n", String.Empty);
            if (String.IsNullOrWhiteSpace(cleanText))
            {
                throw new Exception("Bad formed response!");
            }
            var dto = JsonConvert.DeserializeObject<ResponseDto>(cleanText);
            var foundAppointment = FindClosestAppointmentWithin(dto?.Data, 100);
            if (foundAppointment == default)
            {
                return;
            }
            var message = GenerateMessage(foundAppointment);
            await messageCollector.AddAsync(message);

        }

        private SendGridMessage GenerateMessage(AppointmentDto foundAppointment)
        {
            var mailBody = GenerateMail(foundAppointment);

            var message = new SendGridMessage();
            message.AddTo("zekeriyakocairi@gmail.com");
            message.AddContent("text/html", mailBody);
            message.SetFrom(new EmailAddress("zekeriyakocairi1@gmail.com"));
            message.SetSubject("New Appointment Alert!");
            return message;

        }

        private string BuildUrl(string localtion = "DOC", int personCount = 1)
        {
            string urlPattern = "https://oap.ind.nl/oap/api/desks/DH/slots/?productKey={0}&persons={1}";
            return String.Format(urlPattern, localtion, personCount);


        }
        private AppointmentDto FindClosestAppointmentWithin(IEnumerable<AppointmentDto> appointments, int maxDays = 30)
        {
            if (appointments == default && appointments.Count() == 0)
                return default;
            return appointments.OrderBy(a => a.Date).FirstOrDefault(a => a.Date <= DateTime.Now.AddDays(maxDays));
        }
        private string GenerateMail(AppointmentDto appointment)
        {
            return $"Appointment found at {appointment.Date}";
        }
        private class ResponseDto
        {
            [JsonProperty("status")]
            public string Status { get; set; }
            [JsonProperty("data")]
            public IEnumerable<AppointmentDto> Data { get; set; }
        }

        private class AppointmentDto
        {
            [JsonProperty("key")]
            public string Key { get; set; }
            [JsonProperty("date")]
            public DateTime Date { get; set; }
            [JsonProperty("startTime")]
            public TimeSpan StartTime { get; set; }
            [JsonProperty("endTime")]
            public TimeSpan Endtime { get; set; }
            [JsonProperty("parts")]
            public int Parts { get; set; }
        }
    }

}
