using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class ImprovementPlanByBudgetDTO
    {
        public string Title { get; set; }
    }

    public class ImprovementPlanByBudgetDetailsDTO
    {
        public string Province { get; set; }
        public DateTime EmissionDate { get; set; }
        public string ImprovementPlanType { get; set; }
        public string CUE { get; set; }
        public string InstitutionJurisdiction { get; set; }
        public string Department { get; set; }
        public string Locality { get; set; }
        public string ImprovementPlanCode { get; set; }
        public string ImprovementPlanStatusText { get; set; }
        public decimal TotalRequested { get; set; }
        public string FileNumber { get; set; }
        public string FieldDate { get; set; }
        public string DictumDate { get; set; }
        public string DictumNumber { get; set; }
        public decimal Field_I_Line_A_Inventoried { get; set; }
        public decimal Field_I_Line_A_NonInventoried { get; set; }
        public decimal Field_I_Line_B_Inventoried { get; set; }
        public decimal Field_I_Line_B_NonInventoried { get; set; }
        public decimal Field_I_Line_C_Inventoried { get; set; }
        public decimal Field_I_Line_C_NonInventoried { get; set; }
        public decimal Field_I_Line_D_Inventoried { get; set; }
        public decimal Field_I_Line_D_NonInventoried { get; set; }
        public decimal Field_I_Line_E_Inventoried { get; set; }
        public decimal Field_I_Line_E_NonInventoried { get; set; }
        public decimal Field_I_Line_F_Inventoried { get; set; }
        public decimal Field_I_Line_F_NonInventoried { get; set; }
        public decimal Field_I_Line_G_Inventoried { get; set; }
        public decimal Field_I_Line_G_NonInventoried { get; set; }
        public decimal Field_II_Line_A_Inventoried { get; set; }
        public decimal Field_II_Line_A_NonInventoried { get; set; }
        public decimal Field_II_Line_B_Inventoried { get; set; }
        public decimal Field_II_Line_B_NonInventoried { get; set; }
        public decimal Field_II_Line_C_Inventoried { get; set; }
        public decimal Field_II_Line_C_NonInventoried { get; set; }
        public decimal Field_III_Line_A_Inventoried { get; set; }
        public decimal Field_III_Line_A_NonInventoried { get; set; }
        public decimal Field_III_Line_B_Inventoried { get; set; }
        public decimal Field_III_Line_B_NonInventoried { get; set; }
        public decimal Field_III_Line_C_Inventoried { get; set; }
        public decimal Field_III_Line_C_NonInventoried { get; set; }
        public decimal Field_III_Line_D_Inventoried { get; set; }
        public decimal Field_III_Line_D_NonInventoried { get; set; }
        public decimal Field_III_Line_E_Inventoried { get; set; }
        public decimal Field_III_Line_E_NonInventoried { get; set; }
        public decimal Field_III_Line_F_Inventoried { get; set; }
        public decimal Field_III_Line_F_NonInventoried { get; set; }
        public decimal Field_III_Line_G_Inventoried { get; set; }
        public decimal Field_III_Line_G_NonInventoried { get; set; }
        public decimal Field_III_Line_Ad_Inventoried { get; set; }
        public decimal Field_III_Line_Ad_NonInventoried { get; set; }
        public decimal Field_IV_Line_A_Inventoried { get; set; }
        public decimal Field_IV_Line_A_NonInventoried { get; set; }
        public decimal Field_IV_Line_B_Inventoried { get; set; }
        public decimal Field_IV_Line_B_NonInventoried { get; set; }
        public decimal Field_IV_Line_C_Inventoried { get; set; }
        public decimal Field_IV_Line_C_NonInventoried { get; set; }
        public decimal Field_IV_Line_D_Inventoried { get; set; }
        public decimal Field_IV_Line_D_NonInventoried { get; set; }
        public decimal Field_IV_Line_E_Inventoried { get; set; }
        public decimal Field_IV_Line_E_NonInventoried { get; set; }
        public decimal Field_V_Line_A_Inventoried { get; set; }
        public decimal Field_V_Line_A_NonInventoried { get; set; }
        public decimal Field_V_Line_B_Inventoried { get; set; }
        public decimal Field_V_Line_B_NonInventoried { get; set; }
        public decimal Field_V_Line_C_Inventoried { get; set; }
        public decimal Field_V_Line_C_NonInventoried { get; set; }
        public decimal Field_VI_Line_A_Inventoried { get; set; }
        public decimal Field_VI_Line_A_NonInventoried { get; set; }
        public decimal Field_VI_Line_B_Inventoried { get; set; }
        public decimal Field_VI_Line_B_NonInventoried { get; set; }
        public decimal Field_VI_Line_C_Inventoried { get; set; }
        public decimal Field_VI_Line_C_NonInventoried { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalApproved { get; set; }
        public decimal Dismissed { get; set; }
        public decimal Canceled { get; set; }
        public decimal Rejected { get; set; }
        public decimal Reassigned { get; set; }
        public decimal Eligibility { get; set; }
        public string Resolutions { get; set; }
        public decimal WithoutDictum { get; set; }
        public decimal WithoutResolution { get; set; }
    }
}
