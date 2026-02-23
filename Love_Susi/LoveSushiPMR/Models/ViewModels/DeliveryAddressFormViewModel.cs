using System.ComponentModel.DataAnnotations;

namespace LoveSushiPMR.Models.ViewModels
{
    public class DeliveryAddressFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите город")]
        [Display(Name = "Город")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите улицу")]
        [Display(Name = "Улица")]
        [StringLength(100)]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите номер дома")]
        [Display(Name = "Дом")]
        [StringLength(20)]
        public string House { get; set; } = string.Empty;

        [Display(Name = "Квартира")]
        [StringLength(10)]
        public string? Apartment { get; set; }

        [Display(Name = "Подъезд")]
        [StringLength(5)]
        public string? Entrance { get; set; }

        [Display(Name = "Этаж")]
        [StringLength(5)]
        public string? Floor { get; set; }

        [Display(Name = "Домофон")]
        [StringLength(20)]
        public string? Intercom { get; set; }

        [Display(Name = "Комментарий для курьера")]
        [StringLength(500)]
        public string? Comment { get; set; }

        [Display(Name = "Адрес по умолчанию")]
        public bool IsDefault { get; set; }

        [Required(ErrorMessage = "Выберите зону доставки")]
        [Display(Name = "Зона доставки")]
        public int DeliveryZoneId { get; set; }
    }
}
