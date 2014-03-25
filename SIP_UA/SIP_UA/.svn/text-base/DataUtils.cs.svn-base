using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace LumiSoft.SIP.UA
{
    /// <summary>
    /// This class provides Data related utility methods.
    /// </summary>
    public class DataUtils
    {
        #region static method GetValue

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static object GetValue(DataRow dr,string fieldName)
        {
            if(dr == null){
                throw new ArgumentNullException("dr");
            }
            if(fieldName == null){
                throw new ArgumentNullException("fieldName");
            }
            if(fieldName == ""){
                throw new ArgumentException("Argument 'fieldName' value must be specified.");
            }
            if(!dr.Table.Columns.Contains(fieldName)){
                throw new ArgumentException("Specified fieldName '" +fieldName + "' does not exist in table '" + dr.Table.TableName + "'.");
            }

            return dr[fieldName];
        }

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static object GetValue(DataRowView dr,string fieldName)
        {
            if(dr == null){
                throw new ArgumentNullException("dr");
            }
            if(fieldName == null){
                throw new ArgumentNullException("fieldName");
            }
            if(fieldName == ""){
                throw new ArgumentException("Argument 'fieldName' value must be specified.");
            }
            if(!dr.DataView.Table.Columns.Contains(fieldName)){
                throw new ArgumentException("Specified fieldName '" +fieldName + "' does not exist in table '" + dr.DataView.Table.TableName + "'.");
            }

            return dr[fieldName];
        }

        #endregion

        #region method GetValueString

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static string GetValueString(DataRow dr,string fieldName)
        {
            return GetValue(dr,fieldName).ToString();
        }

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static string GetValueString(DataRowView dr,string fieldName)
        {
            return GetValue(dr,fieldName).ToString();
        }

        #endregion

        #region method GetValueDateTime

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static DateTime GetValueDateTime(DataRow dr,string fieldName)
        {
            object value = GetValue(dr,fieldName);
            if(value is DBNull){
                return DateTime.MinValue;
            }
            else{
                return Convert.ToDateTime(GetValue(dr,fieldName));
            }
        }

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static DateTime GetValueDateTime(DataRowView dr,string fieldName)
        {
            object value = GetValue(dr,fieldName);
            if(value is DBNull){
                return DateTime.MinValue;
            }
            else{
                return Convert.ToDateTime(GetValue(dr,fieldName));
            }
        }

        #endregion

        #region method GetValueInt

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <param name="defaultValue">Default value if value not specified in data row.</param>
        /// <returns>Returns specified field value.</returns>
        public static int GetValueInt(DataRow dr,string fieldName)
        {
            return GetValueInt(dr,fieldName,0);
        }

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <param name="defaultValue">Default value if value not specified in data row.</param>
        /// <returns>Returns specified field value.</returns>
        public static int GetValueInt(DataRow dr,string fieldName,int defaultValue)
        {
            object value = GetValue(dr,fieldName);
            if(value is DBNull){
                return 0;
            }
            else{
                return Convert.ToInt32(GetValue(dr,fieldName));
            }
        }

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static int GetValueInt(DataRowView dr,string fieldName)
        {
            object value = GetValue(dr,fieldName);
            if(value is DBNull){
                return 0;
            }
            else{
                return Convert.ToInt32(GetValue(dr,fieldName));
            }
        }

        #endregion

        #region method GetValueDecimal

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static decimal GetValueDecimal(DataRow dr,string fieldName)
        {
            object value = GetValue(dr,fieldName);
            if(value is DBNull){
                return 0;
            }
            else{
                return Convert.ToInt32(GetValue(dr,fieldName));
            }
        }

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static decimal GetValueDecimal(DataRowView dr,string fieldName)
        {
            object value = GetValue(dr,fieldName);
            if(value is DBNull){
                return 0;
            }
            else{
                return Convert.ToDecimal(GetValue(dr,fieldName));
            }
        }

        #endregion

        #region method GetValueBool

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static bool GetValueBool(DataRow dr,string fieldName)
        {
            object value = GetValue(dr,fieldName);
            if(value is DBNull){
                return false;
            }
            else{
                return Convert.ToBoolean(GetValue(dr,fieldName));
            }
        }

        /// <summary>
        /// Gets specified field value from the specified DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView from where to get value.</param>
        /// <param name="fieldName">Field name in DataRowView.</param>
        /// <returns>Returns specified field value.</returns>
        public static bool GetValueBool(DataRowView dr,string fieldName)
        {
            object value = GetValue(dr,fieldName);
            if(value is DBNull){
                return false;
            }
            else{
                return Convert.ToBoolean(GetValue(dr,fieldName));
            }
        }

        #endregion


        #region staitc method CopyDataRow

		/// <summary>
		/// Copies DataRow values to another DataRow. NOTE: sourceRow must contain all destRow columns, 
		/// sourceRow may contain more fields than destRow.
		/// </summary>
		/// <param name="sourceRow">Source row from which to copy from.</param>
		/// <param name="destRow">Destination row which to copy new values.</param>
		public static void CopyDataRow(DataRow sourceRow,DataRowView destRow)
		{
            if(sourceRow == null){
                throw new ArgumentNullException("sourceRow");
            }
            if(destRow == null){
                throw new ArgumentNullException("destRow");
            }

			foreach(DataColumn dc in destRow.DataView.Table.Columns){
                if(sourceRow.Table.Columns.Contains(dc.ColumnName)){
				    destRow[dc.ColumnName] = sourceRow[dc.ColumnName];
                }
                else{
                    throw new ArgumentException("'sourceRow' does not contain column '" + dc.ColumnName + "'.");
                }
			}
		}

		#endregion

    }
}
