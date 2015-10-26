using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Data.OleDb;
using System.Windows.Forms;

namespace Server
{
    class UDPSender : UDPSend
    {
        //структура пользователя
        private struct User
        {
            public string name;
            public string ip;
        }

        //список пользователей чата
        private List<User> users = new List<User>();

        private IPEndPoint endPoint;
        private UdpClient udp;
        private string hostName;
        private int input, output;

        private Action<string> LogFun, AddUserFun, DelUserFun;
        private Action<int,int,int> DownloadFun;

        public UDPSender(string hostName, int input=25, int output=81)
        {
            this.hostName = hostName;
            this.input = input;
            this.output = output;
            endPoint = new IPEndPoint(IPAddress.Parse(this.hostName),output);
            udp = new UdpClient(endPoint);
        }

        //добавление пользователей
        private void AddUser(string user_name, string ip)
        {
            User newUser = new User();
            newUser.name = user_name;
            newUser.ip = ip;
            users.Add(newUser);
            AddUserFun(user_name);
        }

        //удаление пользователей
        private void DelUser(string user_name, string ip)
        {
            User deleteUser = new User();
            deleteUser.name = user_name;
            deleteUser.ip = ip;
            users.Remove(deleteUser);
            DelUserFun(user_name);
        }

        //рассылка списка подключенных пользователей
        private void SendAllUsers(string ip)
        {
            for (int i = 0; i < users.Count; i++)
                Send("clear/all",ip,input);

            for (int i = 0; i < users.Count; i++)
                if (users[i].ip != ip)
                    Send("add/" + users[i].name, ip, input);
        }

        //функция рассылки сообщений всем пользователям
        private void SendMessageAll(string msg, string from)
        {
            for (int i = 0; i < users.Count; i++)
            {
                Send("msg/" + from + "/" + msg, users[i].ip, input);
            }
        }

        //отключение пользователя от сервера
        public void KickUser(string name)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].name == name)
                {
                    Send("kick/",users[i].ip,input);
                    DelUser(users[i].name, users[i].ip);
                    break;
                }
            }
        }

        private string name;

        /*
         * Функция парсинга входящих сообщений
         */
        private void Parse(ref string m)
        {
            string[] str = m.Split('/');
            switch (str[0])
            {
                /*
                 * Функции подключения и отключения пользователя
                 */
                case "connect":     Send("ok/" + str[1], str[2], input);         //подключаем пользователя
                                    AddUser(str[1],str[2]);
                                    LogFun("Пользватель: " + str[1] + " подключился. ["+str[2]+']');
                                    Send("msg/Сервер/Добро пожаловать " + str[1] + "!", str[2], input);
                                    SendMessageAll(str[1] + " подключился.", "Сервер");
                                    break;
                case "disconnect":  Send("neok/" + str[1], str[2], input);     //отсоединяем пользователя
                                    DelUser(str[1], str[2]);
                                    LogFun("Пользватель: " + str[1] + " отключился [" + str[2] + ']');
                                    Send("msg/Сервер/До свидания, " + str[1] + ".", str[2], input);
                                    SendMessageAll(str[1] + " отключился.", "Сервер");
                                    break;
                case "get":         SendAllUsers(str[2]); break;    //запрос на получение списка пользователей
                //пересылка сообщения всем пользователям
                case "msg":         SendMessageAll(str[2], str[1]); LogFun(str[1] + "> " + str[2]); break;
                //загрузка и передача файла
                case "f":           name = Properties.Settings.Default.SaveDirectory+"\\"+str[1]; 
                                    LogFun(">Загрузка файла: "+str[1]);
                                    //запись в файла полученного сообщения
                                    BinaryWriter f = new BinaryWriter(File.Open(name,FileMode.OpenOrCreate));
                                    DownloadFun(0,str[2].Length,10);
                                    foreach (var i in str[2])
                                        f.Write(i);
                                    f.Close();
                                    LogFun(">Файл загружен!");
                                    //пересылка файла пользователям
                                    foreach (var i in users)
                                        if (i.ip != str[3])
                                            Send(m,i.ip,input);
                                    break;
                /*
                 * Функции регистрации и входа пользователя
                 */
                case "reguser":     {   //регистрация нового пользователя
                                        OleDbConnection conn = new OleDbConnection(Properties.Settings.Default.baseConnectionString);
                                        conn.Open();
                                        OleDbCommand checkUser = conn.CreateCommand();
                                        checkUser.CommandText = "SELECT * FROM Users WHERE Login = '" + str[1].ToString() + "'";
                                        int r = Convert.ToInt32(checkUser.ExecuteScalar());
                                        if (r > 0)
                                        {
                                            Send("regnok/Ошибка такой пользователь уже существует!", str[3], input);
                                        }
                                        else {
                                            checkUser.CommandText = "SELECT MAX(ID) FROM Users";
                                            var psId = checkUser.ExecuteScalar();
                                            int id = 1;
                                            if (psId != DBNull.Value)
                                                id += Convert.ToInt32(psId);

                                            checkUser.CommandText = "insert into Users values ('"+id.ToString()+"','" 
                                                + str[1].ToString() + "','" + str[2].ToString() + "')";
                                            int rez = Convert.ToInt32(checkUser.ExecuteNonQuery());
                                            if (rez > 0)
                                            {
                                                LogFun("Пользователь " + str[1] + " зарегестрировался!");
                                                Send("regok/Регистрация прошла успешно!", str[3], input);
                                            }
                                        }
                                        conn.Close();
                                    } break;
                case "entuser":     {                                           //вход пользователя в чат
                                        OleDbConnection conn = new OleDbConnection(Properties.Settings.Default.baseConnectionString);
                                        conn.Open();
                                        OleDbCommand checkUser = conn.CreateCommand();
                                        checkUser.CommandText = "SELECT * FROM Users WHERE Login = '" + str[1].ToString()
                                            + "' AND Password = '" + str[2].ToString()+"'";
                                        int r = Convert.ToInt32(checkUser.ExecuteScalar());
                                        if (r > 0)
                                            Send("entok/",str[3],input);
                                        else
                                            Send("entnok/Неверное имя пользователя или пароль!",str[3],input);
                                    } break;
            }
        }

        /*
         * Свойства объекта
         */

        //тут задается функция вывода сообщений на экран
        public Action<string> setLogFun
        {
            set { LogFun = value; }
        }

        //тут задается функция добавления пользователей
        public Action<string> setAddUserFun
        {
            set { AddUserFun = value; }
        }

        //тут задается функция удаления пользователей
        public Action<string> setDelUserFun
        {
            set { DelUserFun = value; }
        }

        //функция отображения информации о загрузке файла
        public Action<int,int,int> setDownloadFun
        {
            set { DownloadFun = value; }
        }

        //отправка сообщения
        public virtual void Send(string msg, string ip, int port)
        {
            //получаем поток байт
            byte[] data = Encoding.Default.GetBytes(msg);
            //отправляем его пользователю
            udp.Send(data, data.Length, ip, port);
        }

        //слушаем входящие сообщения
        public virtual void Recieve()
        {
            try
            {
                while (true)
                {
                    IPEndPoint point = null;                //конечная точка
                    byte[] data = udp.Receive(ref point);   //получаем поток байт
                    //преобразуем байты в строку
                    string message = Encoding.Default.GetString(data) + "/" + point.Address.ToString();
                    Parse(ref message);                     //парсим строку
                }
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.ToString()+"\nПерезапустите приложение!","Ошибка",
                    MessageBoxButtons.OK,MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }
}
