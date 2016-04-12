// *******************************************************************************
// * Copyright (c) 2012,2013,2014 VisiSonics, Inc.
// * This software is the proprietary information of VisiSonics, Inc.
// * All Rights Reserved.
// *
// * © VisiSonics Corporation, 2013-2014
// * VisiSonics Confidential Information
// * Source code provided under the terms of the Software License Agreement 
// * between VisiSonics Corporation and Oculus VR, LLC dated 09/10/2014
// ********************************************************************************
// 
// Original Author: R E Haxton
// $Author$
// $Date$
// $LastChangedDate$
// $Revision$
//
// Purpose:
//
// Comments: 
// 

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Runtime.CompilerServices;
using RealSpace3D;
using RealSpace3DPlatformSwitcher;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle("RealSpace3D")]
[assembly: AssemblyDescription("VisiSonics RealSpace3D Audio Engine - Unity Plugin")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("VisiSonics, Inc")]
[assembly: AssemblyProduct("RealSpace3D")]
[assembly: AssemblyCopyright("VisiSonics, Inc")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion("0.11.12.*")]

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

namespace RealSpace3DAbout
{
	public class RealSpace3D_About : MonoBehaviour
	{
		private static RealSpace3D_PlatformSwitcher _theSwitcher = RealSpace3D_PlatformSwitcher.Instance;

		[MenuItem("Help/RealSpace3D/About", false, 901)]

		/// <summary>
		/// Init this instance. Shows build date and version number.
		/// </summary>
		private static void Init()
		{
#if UNITY_IPHONE
			string sNotice = "(iOS) RealSpace3D version: 0.11.12\nvsEngine version: 4.3.20160204";
#else
			int nBuildDate = 	_theSwitcher.GetBuildDate();
			int nVersionMajor = _theSwitcher.GetVersionMajor();
			int nVersionMinor = _theSwitcher.GetVersionMinor();

			GetRS3DVersion theRS3DVersion = new GetRS3DVersion();
		
			string sNotice = "RealSpace3D version: " + theRS3DVersion.FormatVersion() + "\nVsEngine version: v" + nVersionMajor.ToString() + "." + nVersionMinor.ToString() + "." + nBuildDate.ToString() + "\n\nPlease visit www.realspace3daudio.com forum for updates and answers. Request license, report bugs, or direct general questions to support@visisonics.com" + "\n\nVisiSonics, Inc. Copyright 2011 - 2016";
#endif

			EditorUtility.DisplayDialog("RealSpace3D Copyright 2011 - 2016", sNotice, "Ok");
		}

		public class GetRS3DVersion
		{
			string sVersion;
	
			public string Version 
			{
				get 
				{
					if(sVersion == null)
					{
						sVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
					}
			
					return sVersion;
				}
			}
	
			public string FormatVersion()
			{
				return string.Format("v{0}", Version);
			}
		}
	}
}