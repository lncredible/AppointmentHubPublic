namespace AppointmentManagementSystem.Services
{
    public class SmsAutomationService : IHostedService, IDisposable
    {
        #region Global Variables
        private TimeSpan _tsTimerPeriod = TimeSpan.FromSeconds(10);
        private Timer? _tTimer;
        private readonly IServiceScopeFactory _oScopeFactory;
        #endregion

        #region Constructor
        public SmsAutomationService(IServiceScopeFactory oScopeFactory)
        {
            _oScopeFactory = oScopeFactory;
        }
        #endregion

        #region Methods
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _tTimer = new Timer(InvokeSmsSystem, null, TimeSpan.Zero, _tsTimerPeriod);
            return Task.CompletedTask;
        }

        private void InvokeSmsSystem(object oState)
        {
            using (var vScope = _oScopeFactory.CreateScope())
            {
                var vSmsService = vScope.ServiceProvider.GetRequiredService<ISmsService>();
                vSmsService.StoreAppointmentsDue();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _tTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _tTimer?.Dispose();
        }
        #endregion
    }
}
