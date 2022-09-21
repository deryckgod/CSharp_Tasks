using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer_File
{
    internal class Move_File
    {
        public string MoveFile(String fileFullPath, String destinationPath)
        {
            string returnString = "";

            string fileName = fileFullPath.Split('\\').Last();
            try
            {
                File.Move(fileFullPath, destinationPath + fileName);
                returnString += String.Format("{0} \t 從{1} \r\n 轉移至新位置{2} \r\n", fileName, fileFullPath, (destinationPath + fileName));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
           
            return returnString;
        }
    }
}
