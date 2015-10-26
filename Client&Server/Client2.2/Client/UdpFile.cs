using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Client
{
    class UdpFile
    {
        private string file;    //путь к файлу
        private string name;    //имя файла
        private long length;    //рзмер файла
        
        /*
         * Свойства объекта
         */
        
        public string FileName
        {
            get { return name; }    //возвращает имя файла
        }
        
        public long FileLen
        {
            get { return length; }  //возвращает размер файла
        }

        //конструктор объекта
        public UdpFile(string file)
        {
            this.file = file;
            //получаем информацию о файле
            FileInfo info = new FileInfo(file);
            name = info.Name;       //имя файла
            length = info.Length;   //размер файла
        }

        /*
         * В этой функции происходит преобразование имени файла и его данных,
         * в общий поток байт.
         * Это сделано для удобства посылки файлов по сети.
         */
        public byte[] getBytes()
        {
            //побайтовое чтение файла в массив
            BinaryReader r = new BinaryReader(File.Open(file,FileMode.Open));
            byte[] data = new byte[length];
            r.Read(data, 0, data.Length);
            r.Close();

            //преобразование массива байт в строку, как уже было написано выше
            //это сделано для удобства рассылки файлов.
            string text = "f/"+name+"/"+Encoding.Default.GetString(data);

            //преобразование имени файла и его данных в байты
            byte[] sendData = Encoding.Default.GetBytes(text);

            return sendData;
        }
    }
}
