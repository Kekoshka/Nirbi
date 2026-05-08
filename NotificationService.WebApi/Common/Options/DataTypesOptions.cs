namespace NotificationService.WebApi.Common.Options
{
    /// <summary>
    /// Настройки типов данных, в частности decimal
    /// </summary>
    public class DataTypesOptions
    {
        /// <summary>
        /// Расрешение типа decimal
        /// </summary>
        public int DecimalScale { get; set; }
        
        /// <summary>
        /// Точность типа decimal
        /// </summary>
        public int DecimalPrecision { get; set; }
    }
}
