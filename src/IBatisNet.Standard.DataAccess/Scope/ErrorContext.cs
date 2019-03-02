#region Apache Notice

/*****************************************************************************
 * $Header: $
 * $Revision: 383115 $
 * $Date: 2006-03-04 15:21:51 +0100 (sam., 04 mars 2006) $
 * 
 * iBATIS.NET Data Mapper
 * Copyright (C) 2004 - Gilles Bayon
 *  
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 ********************************************************************************/

#endregion

#region Using

using System;
using System.Text;

#endregion

namespace IBatisNet.DataAccess.Scope
{
    /// <summary>
    ///     An error context to help us create meaningful error messages.
    /// </summary>
    public class ErrorContext
    {
        #region Fields

        #endregion

        #region Properties

        /// <summary>
        ///     The resource causing the problem
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        ///     The activity that was happening when the error happened
        /// </summary>
        public string Activity { get; set; }

        /// <summary>
        ///     The object ID where the problem happened
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        ///     More information about the error
        /// </summary>
        public string MoreInfo { get; set; }

        /// <summary>
        ///     The cause of the error
        /// </summary>
        public Exception Cause { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Clear the error context
        /// </summary>
        public void Reset()
        {
            Resource = string.Empty;
            Activity = string.Empty;
            ObjectId = string.Empty;
            MoreInfo = string.Empty;
            Cause = null;
        }


        /// <summary>
        ///     ToString method for ErrorContext
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder message = new StringBuilder();


            // activity
            if (Activity != null && Activity.Length > 0)
            {
                message.Append("\n- The error occurred while ");
                message.Append(Activity);
                message.Append(".");
            }

            // more info
            if (MoreInfo != null && MoreInfo.Length > 0)
            {
                message.Append("\n- ");
                message.Append(MoreInfo);
            }

            // resource
            if (Resource != null && Resource.Length > 0)
            {
                message.Append("\n- The error occurred in ");
                message.Append(Resource);
                message.Append(".");
            }

            // object
            if (ObjectId != null && ObjectId.Length > 0)
            {
                message.Append("  \n- Check the ");
                message.Append(ObjectId);
                message.Append(".");
            }

            // cause
            if (Cause != null)
            {
                message.Append("\n- Cause: ");
                message.Append(Cause);
            }

            return message.ToString();
        }

        #endregion
    }
}