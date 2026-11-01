using System;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class CustomerOpeningBalance
    {
        public long CustomerOpeningBalanceID { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
        [Display(Name = "Customer")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Opening Date is required.")]
        [Display(Name = "Opening Date")]
        [DataType(DataType.Date)]
        public DateTime OpeningDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Balance Amount is required.")]
        [Display(Name = "Balance Amount")]
        public decimal BalanceAmount { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display
        public string? CustomerName { get; set; }
        public string? Operation { get; set; }
    }

    public class CustomerDeposit
    {
        public long CustomerDepositID { get; set; }

        [Required(ErrorMessage = "Deposit No is required.")]
        [StringLength(30)]
        [Display(Name = "Deposit No")]
        public string DepositNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer is required.")]
        [Display(Name = "Customer")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Deposit Date is required.")]
        [Display(Name = "Deposit Date")]
        [DataType(DataType.Date)]
        public DateTime DepositDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [StringLength(30)]
        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; } = "Cash"; // Cash, Bank, Cheque, MFS

        [StringLength(100)]
        [Display(Name = "Reference No / Cheque No")]
        public string? ReferenceNo { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Posted";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display
        public string? CustomerName { get; set; }
        public string? Operation { get; set; }
    }

    public class CustomerCommission
    {
        public long CustomerCommissionID { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
        [Display(Name = "Customer")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Commission Date is required.")]
        [Display(Name = "Commission Date")]
        [DataType(DataType.Date)]
        public DateTime CommissionDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Commission Type is required.")]
        [StringLength(50)]
        [Display(Name = "Commission Type")]
        public string CommissionType { get; set; } = "Monthly Target";

        [StringLength(50)]
        [Display(Name = "Reference No")]
        public string? ReferenceNo { get; set; }

        [Required(ErrorMessage = "Commission Amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be positive.")]
        [Display(Name = "Commission Amount")]
        public decimal CommissionAmount { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Posted";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display
        public string? CustomerName { get; set; }
        public string? Operation { get; set; }
    }
}

