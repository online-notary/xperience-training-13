//--------------------------------------------------------------------------------------------------
// <auto-generated>
//
//     This code was generated by code generator tool.
//
//     To customize the code use your own partial class. For more info about how to use and customize
//     the generated code see the documentation at http://docs.kentico.com.
//
// </auto-generated>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CMS;
using CMS.Base;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Types.MedioClinic;

[assembly: RegisterDocumentType(CompanyService.CLASS_NAME, typeof(CompanyService))]

namespace CMS.DocumentEngine.Types.MedioClinic
{
	/// <summary>
	/// Represents a content item of type CompanyService.
	/// </summary>
	public partial class CompanyService : TreeNode
	{
		#region "Constants and variables"

		/// <summary>
		/// The name of the data class.
		/// </summary>
		public const string CLASS_NAME = "MedioClinic.CompanyService";


		/// <summary>
		/// The instance of the class that provides extended API for working with CompanyService fields.
		/// </summary>
		private readonly CompanyServiceFields mFields;

		#endregion


		#region "Properties"

		/// <summary>
		/// CompanyServiceID.
		/// </summary>
		[DatabaseIDField]
		public int CompanyServiceID
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("CompanyServiceID"), 0);
			}
			set
			{
				SetValue("CompanyServiceID", value);
			}
		}


		/// <summary>
		/// Service description.
		/// </summary>
		[DatabaseField]
		public string ServiceDescription
		{
			get
			{
				return ValidationHelper.GetString(GetValue("ServiceDescription"), @"");
			}
			set
			{
				SetValue("ServiceDescription", value);
			}
		}


		/// <summary>
		/// Icon.
		/// </summary>
		[DatabaseField]
		public Guid Icon1
		{
			get
			{
				return ValidationHelper.GetGuid(GetValue("Icon"), Guid.Empty);
			}
			set
			{
				SetValue("Icon", value);
			}
		}


		/// <summary>
		/// Gets an object that provides extended API for working with CompanyService fields.
		/// </summary>
		[RegisterProperty]
		public CompanyServiceFields Fields
		{
			get
			{
				return mFields;
			}
		}


		/// <summary>
		/// Provides extended API for working with CompanyService fields.
		/// </summary>
		[RegisterAllProperties]
		public partial class CompanyServiceFields : AbstractHierarchicalObject<CompanyServiceFields>
		{
			/// <summary>
			/// The content item of type CompanyService that is a target of the extended API.
			/// </summary>
			private readonly CompanyService mInstance;


			/// <summary>
			/// Initializes a new instance of the <see cref="CompanyServiceFields" /> class with the specified content item of type CompanyService.
			/// </summary>
			/// <param name="instance">The content item of type CompanyService that is a target of the extended API.</param>
			public CompanyServiceFields(CompanyService instance)
			{
				mInstance = instance;
			}


			/// <summary>
			/// CompanyServiceID.
			/// </summary>
			public int ID
			{
				get
				{
					return mInstance.CompanyServiceID;
				}
				set
				{
					mInstance.CompanyServiceID = value;
				}
			}


			/// <summary>
			/// Service description.
			/// </summary>
			public string ServiceDescription
			{
				get
				{
					return mInstance.ServiceDescription;
				}
				set
				{
					mInstance.ServiceDescription = value;
				}
			}


			/// <summary>
			/// Icon.
			/// </summary>
			public DocumentAttachment Icon
			{
				get
				{
					return mInstance.GetFieldDocumentAttachment("Icon");
				}
			}
		}

		#endregion


		#region "Constructors"

		/// <summary>
		/// Initializes a new instance of the <see cref="CompanyService" /> class.
		/// </summary>
		public CompanyService() : base(CLASS_NAME)
		{
			mFields = new CompanyServiceFields(this);
		}

		#endregion
	}
}