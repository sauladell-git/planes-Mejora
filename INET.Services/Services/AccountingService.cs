using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using INET.Core.Enums;
using INET.Data;
using INET.Services.DTO;
using INET.Utils.Helpers;
using System.Web.Security;
using INET.Core.Constants;
using INET.Utils.Helpers.Permissions;
using System.Data.Entity.SqlServer;
using ClosedXML.Excel;

namespace INET.Services
{
    public class AccountingService:BusinessService
    {
        public AccountingService(INETContext context)
        {
            Context = context;
            
        }
        /// <summary>
        /// graba registro de cuenta
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
       
        /// <returns>True en caso de que sea válido asignar ese cue a ese solicitado. False en caso contrario.</returns>
        public bool Save(int? id,int improvementPlanId,decimal aprovedAmount,decimal? rejectedAmount,string comments, DateTime date, int expenditureObjectTypeId,string number,int dictumId)
        {
            try
            {
                var accountRendering = new AccountRendering();
                if (id != null)
                {
                    accountRendering = (from D in Context.AccountRenderings
                                        where D.AccountingRenderingID == id
                                        select D).FirstOrDefault();
                }
                accountRendering.AprovedAmount = aprovedAmount;
                accountRendering.Date = date;
                accountRendering.ExpenditureObjectTypeId = expenditureObjectTypeId;
                accountRendering.ImprovementPlanId = improvementPlanId;
                accountRendering.Number = number;
                accountRendering.DictumId = dictumId;
                accountRendering.RejectedAmount = rejectedAmount;
                accountRendering.Comments = comments;
                if (id == null)
                {
                    Context.AccountRenderings.Add(accountRendering);

                }
                Context.SaveChanges();
                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }

       public AccountRenderingDTO Get (int Id)
        {
            AccountRenderingDTO item = new AccountRenderingDTO();
            var accountRendering = new AccountRendering();

            accountRendering = (from D in Context.AccountRenderings
                                where D.AccountingRenderingID == Id
                                select D).FirstOrDefault();

            item.aprovedAmount     = accountRendering.AprovedAmount;
            item.comment           = accountRendering.Comments;
            item.date              = accountRendering.Date;
            item.DateDay           = accountRendering.Date.Day.ToString();
            item.DateMonth         = accountRendering.Date.Month.ToString();
            item.DateYear          = accountRendering.Date.Year.ToString();
            item.expenditureObjectTypeId = accountRendering.ExpenditureObjectTypeId;
            item.improvementPlanId = accountRendering.ImprovementPlanId;
            item.number            = accountRendering.Number;
            item.dictumId = accountRendering.DictumId.Value;
            item.id = Id;
            if (accountRendering.RejectedAmount != null)
                item.rejectedAmount = accountRendering.RejectedAmount.Value;
            else
                item.rejectedAmount = 0;

            return item;

        }
        public bool Delete(int id)
        {
            try
            {
                var accountRendering = new AccountRendering();
                
                    accountRendering = (from D in Context.AccountRenderings
                                        where D.AccountingRenderingID == id
                                        select D).FirstOrDefault();
                
               
              
                    Context.AccountRenderings.Remove(accountRendering);

                
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Lista los AC de un plan de mejora
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <returns>Una lista con lo AC pertenecientes a ese plan de mejora</returns>
        public IList<AccountRenderingViewDTO> List(int improvementPlanId)
        {
            List<AccountRenderingViewDTO> result = new List<AccountRenderingViewDTO>();
            
            var r= Context.ImprovementPlans
                .Include("AccountRenderings.ExpenditureObjectType")
                .Include("AccountRenderings.Dictum")
                .Where(x => x.Id == improvementPlanId).First().AccountRenderings.OrderByDescending(x => x.AccountingRenderingID).ToList();

            foreach( AccountRendering item in r)
            {
                AccountRenderingViewDTO i = new AccountRenderingViewDTO();
                i.AccountingRenderingID = item.AccountingRenderingID;
                i.ImprovementPlanId = item.ImprovementPlanId;
                i.AprovedAmount = item.AprovedAmount;
                i.ExpenditureObjectType = item.ExpenditureObjectType.Description;
                i.DictumNumber = item.Dictum.DictumNumber;
                i.Date = item.Date;
                i.Number = item.Number;
                if (item.RejectedAmount != null)
                    i.RejectedAmount = item.RejectedAmount.Value;
                else
                    i.RejectedAmount = 0;
                if (item.Comments != null)
                    i.Comments = item.Comments;
                else
                    i.Comments = "";
                result.Add(i);

            }

            return result;
        }
        

        public decimal DictumAmount(Dictum d)
        {
           var accountRendering = (from D in Context.AccountRenderings
                                where D.DictumId == d.Id
                                select D).ToList();
            if (accountRendering.Count>0)
            {
               return  d.Ammount.Value - (accountRendering.Sum(x => x.AprovedAmount)); 
            }else
            {
                return d.Ammount.Value;
            }
        }

     


    }
}
