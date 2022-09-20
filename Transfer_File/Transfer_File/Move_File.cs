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

        #region 備著用的程式(復原檔案位置)
        private void btnTransfer_Click(object sender, EventArgs e)
        {
            //FileStream fileStream = null;

            // 寫法二跟三都是用完關閉文字檔，寫法一最後需要Dispose才能關閉
            // 寫法一
            //fileStream = new FileStream(@"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt", FileMode.Open);
            // 寫法二
            //try { fileStream = new FileStream(@"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt", FileMode.Open); } catch { fileStream.Dispose(); }
            // 寫法三
            //using (fileStream = new FileStream(@"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt", FileMode.Open)) { }

            //moveFile();

        }

        public string restoreFile()
        {
            string returnString = "";
            // ABC.txt 轉出
            string sourceFile = @"D:\Desktop\ALPED\Systex\C#_VS\ABC.txt";
            string destinationFile = @"D:\Desktop\ALPED\Systex\C#_VS\test\ABC.txt";
            File.Move(sourceFile, destinationFile);
            returnString += String.Format("{0} 從{1} \r\n 轉移至新位置{2} \r\n", "ABC.txt", sourceFile, destinationFile);

            // CBA.txt 轉入
            string otherFile = @"D:\Desktop\ALPED\Systex\C#_VS\test\CBA.txt";
            string otherDesFile = @"D:\Desktop\ALPED\Systex\C#_VS\CBA.txt";
            File.Move(otherFile, otherDesFile);
            returnString += String.Format("{0} 從{1} \r\n 轉移至新位置{2} \r\n", "CBA.txt", otherFile, otherDesFile);

            return returnString;
        }
        #endregion
    }

}
