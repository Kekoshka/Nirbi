namespace MinorTaskService.WebApi.Common.Options
{
    public class ValidationOptions
    {
        public const int MinLengthLogin = 5;
        public const int MinLengthPassword = 8;
        public const int MinLengthName = 1;
        public const int MinLengthUserName = 2;
        public const int MinPriority = 1;
        public const int MinValuePositiveNumber = 0;
        public const int MaxLengthLogin = 50;
        public const int MaxLengthPassword = 50;
        public const int MaxLengthName = 100;
        public const int MaxLengthUserName = 50;
        public const int MaxLengthDescription = 500;
        public const int MaxPriority = 10;
        public const int MaxLengthAction = 500;
        public const int MaxLengthMessage = 1000;
        public const int TimeReserveForDelay = 10;
        public const int MaxLatitude = 90;
        public const int MinLatitude = -90;
        public const int MaxLongitude = 180;
        public const int MinLongitude = -180;
        public const int MaxNumberVolunteers = 100;
        public const int MinNumberVolunteers = 1;
        public const int MaxEncouragement = 1000000;
        public const int MinEncouragement = 0;

    }
}
