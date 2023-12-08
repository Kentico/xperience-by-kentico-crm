using Kentico.Xperience.CRM.Dynamics.Dataverse.Entities;

#pragma warning disable CS1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()]

namespace Kentico.Xperience.CRM.Dynamics.Dataverse
{
	
	
	/// <summary>
	/// Represents a source of entities bound to a Dataverse service. It tracks and manages changes made to the retrieved entities.
	/// </summary>
	public partial class OrgContext : Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
	{
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public OrgContext(Microsoft.Xrm.Sdk.IOrganizationService service) : 
				base(service)
		{
		}
		
		/// <summary>
		/// Gets a binding to the set of all <see cref="Contact"/> entities.
		/// </summary>
		public System.Linq.IQueryable<Contact> ContactSet
		{
			get
			{
				return this.CreateQuery<Contact>();
			}
		}
		
		/// <summary>
		/// Gets a binding to the set of all <see cref="Lead"/> entities.
		/// </summary>
		public System.Linq.IQueryable<Lead> LeadSet
		{
			get
			{
				return this.CreateQuery<Lead>();
			}
		}
	}
}
#pragma warning restore CS1591