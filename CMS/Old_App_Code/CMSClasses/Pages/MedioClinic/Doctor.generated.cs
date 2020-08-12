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

[assembly: RegisterDocumentType(Doctor.CLASS_NAME, typeof(Doctor))]

namespace CMS.DocumentEngine.Types.MedioClinic
{
	/// <summary>
	/// Represents a content item of type Doctor.
	/// </summary>
	public partial class Doctor : TreeNode
	{
		#region "Constants and variables"

		/// <summary>
		/// The name of the data class.
		/// </summary>
		public const string CLASS_NAME = "MedioClinic.Doctor";


		/// <summary>
		/// The instance of the class that provides extended API for working with Doctor fields.
		/// </summary>
		private readonly DoctorFields mFields;

		#endregion


		#region "Properties"

		/// <summary>
		/// DoctorID.
		/// </summary>
		[DatabaseIDField]
		public int DoctorID
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("DoctorID"), 0);
			}
			set
			{
				SetValue("DoctorID", value);
			}
		}


		/// <summary>
		/// URL slug.
		/// </summary>
		[DatabaseField]
		public string UrlSlug
		{
			get
			{
				return ValidationHelper.GetString(GetValue("UrlSlug"), @"");
			}
			set
			{
				SetValue("UrlSlug", value);
			}
		}


		/// <summary>
		/// Degree.
		/// </summary>
		[DatabaseField]
		public string Degree
		{
			get
			{
				return ValidationHelper.GetString(GetValue("Degree"), @"");
			}
			set
			{
				SetValue("Degree", value);
			}
		}


		/// <summary>
		/// Specialty.
		/// </summary>
		[DatabaseField]
		public string Specialty
		{
			get
			{
				return ValidationHelper.GetString(GetValue("Specialty"), @"");
			}
			set
			{
				SetValue("Specialty", value);
			}
		}


		/// <summary>
		/// Biography.
		/// </summary>
		[DatabaseField]
		public string Biography
		{
			get
			{
				return ValidationHelper.GetString(GetValue("Biography"), @"");
			}
			set
			{
				SetValue("Biography", value);
			}
		}


		/// <summary>
		/// User account.
		/// </summary>
		[DatabaseField]
		public int UserAccount
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("UserAccount"), 0);
			}
			set
			{
				SetValue("UserAccount", value);
			}
		}


		/// <summary>
		/// Gets an object that provides extended API for working with Doctor fields.
		/// </summary>
		[RegisterProperty]
		public DoctorFields Fields
		{
			get
			{
				return mFields;
			}
		}


		/// <summary>
		/// Provides extended API for working with Doctor fields.
		/// </summary>
		[RegisterAllProperties]
		public partial class DoctorFields : AbstractHierarchicalObject<DoctorFields>
		{
			/// <summary>
			/// The content item of type Doctor that is a target of the extended API.
			/// </summary>
			private readonly Doctor mInstance;


			/// <summary>
			/// Initializes a new instance of the <see cref="DoctorFields" /> class with the specified content item of type Doctor.
			/// </summary>
			/// <param name="instance">The content item of type Doctor that is a target of the extended API.</param>
			public DoctorFields(Doctor instance)
			{
				mInstance = instance;
			}


			/// <summary>
			/// DoctorID.
			/// </summary>
			public int ID
			{
				get
				{
					return mInstance.DoctorID;
				}
				set
				{
					mInstance.DoctorID = value;
				}
			}


			/// <summary>
			/// URL slug.
			/// </summary>
			public string UrlSlug
			{
				get
				{
					return mInstance.UrlSlug;
				}
				set
				{
					mInstance.UrlSlug = value;
				}
			}


			/// <summary>
			/// Degree.
			/// </summary>
			public string Degree
			{
				get
				{
					return mInstance.Degree;
				}
				set
				{
					mInstance.Degree = value;
				}
			}


			/// <summary>
			/// Specialty.
			/// </summary>
			public string Specialty
			{
				get
				{
					return mInstance.Specialty;
				}
				set
				{
					mInstance.Specialty = value;
				}
			}


			/// <summary>
			/// Biography.
			/// </summary>
			public string Biography
			{
				get
				{
					return mInstance.Biography;
				}
				set
				{
					mInstance.Biography = value;
				}
			}


			/// <summary>
			/// User account.
			/// </summary>
			public int UserAccount
			{
				get
				{
					return mInstance.UserAccount;
				}
				set
				{
					mInstance.UserAccount = value;
				}
			}
		}

		#endregion


		#region "Constructors"

		/// <summary>
		/// Initializes a new instance of the <see cref="Doctor" /> class.
		/// </summary>
		public Doctor() : base(CLASS_NAME)
		{
			mFields = new DoctorFields(this);
		}

		#endregion
	}
}