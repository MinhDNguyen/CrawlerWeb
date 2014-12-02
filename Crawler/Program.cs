using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using HtmlAgilityPack;

namespace Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            _URLbase = "http://sis.hust.edu.vn/";
            string select;
            string check;
            string sbjName;
            string contents;
            string idUser;
            string idPass = "";

            Console.OutputEncoding = Encoding.UTF8;
            do
            {
                Console.WriteLine("***** CHƯƠNG TRÌNH LẤY THÔNG TIN MÔN HỌC VÀ THÔNG TIN NGƯỜI DÙNG *****");
                Console.WriteLine("(1). Lấy thông tin về môn học mà bạn muốn biết. ");
                Console.WriteLine("(2). Lấy thông tin cá nhân và tình trạng người sử dụng. ");
                Console.Write("Hãy chọn thông tin muốn biết: ");
                select = Console.ReadLine();
                switch (select)
                {
                    case "1":
                        {
                            Console.WriteLine("\t \t -------------------------------------");
                            Console.Write("Xin mời bạn nhập tên môn học: ");
                            sbjName = Console.ReadLine();
                            contents = Send_POST("/ModuleProgram/CourseLists.aspx", "/ModuleProgram/CourseLists.aspx", GenerateSubjectsPostData(sbjName));
                            if (VerifyWebContent(contents, sbjName))
                            {
                                CrawlerSubjects(contents);
                            }
                            else
                            {
                                Console.WriteLine("Thông tin không hợp lệ");
                            }
                            break;
                        }
                    case "2":
                        {
                            Console.WriteLine("\t \t -------------------------------------");
                            ResetCookies();
                            Console.Write("Gõ tên tài khoản: ");
                            idUser = Console.ReadLine();
                            Console.Write("Gõ mật khẩu: ");
                            idPass = ReadPassword();
                            string temp = Send_POST(null, null, GenerateLoginPostData(idUser, idPass));
                            if (VerifyWebContent(temp, "Xin chào bạn"))
                            {
                                contents = Send_GET("/ModuleUser/UserInformation.aspx", null);
                                CrawlerInfo(contents);
                            }
                            else
                            {
                                Console.WriteLine("\nBạn nhập sai User hoặc Password");
                            }
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Bạn nhập sai vui lòng nhập lại");
                            break;
                        }
                }
                Console.Write("Bạn có muốn tiếp tục không???(Y/N): ");
                check = Console.ReadLine();
                Console.WriteLine("\t \t -------------------------------------");
            } while (check == "y" || check == "Y");
            Console.ReadLine();
        }

        // Mã hóa mật khẩu
        public static string ReadPassword()
        {
            string idPass = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    idPass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && idPass.Length > 0)
                    {
                        idPass = idPass.Substring(0, (idPass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            // kết thúc khi gõ Enter
            while (key.Key != ConsoleKey.Enter);
            return idPass;
        }


        // Cookie của phiên làm việc
        static CookieContainer _cookies = null;
        public static void ResetCookies()
        {
            _cookies = new CookieContainer();
        }

        // Thiết lập URL
        static private String _URLbase;
        public static String URLbase
        {
            get { return Program._URLbase; }
            set
            {
                Program._URLbase = value;
                _URLbase.TrimEnd('/');
                _URLbase.TrimEnd('\\');
            }
        }

        // Tạo ra dữ liệu cho nội dung gói POST, phục vụ cho việc đăng nhập.
        private static String GenerateLoginPostData(String username, String password)
        {
            Dictionary<string, string> login_post_data_list = new Dictionary<string, string>();
            login_post_data_list.Add("__EVENTTARGET", "ctl00%24cLogIn1%24bt_cLogIn");
            login_post_data_list.Add("__VIEWSTATE", "%2FwEPDwUKMjE0MzQwNjk4Mw9kFgJmD2QWAgIDD2QWBAIFD2QWAmYPPCsABAEADxYCHgVWYWx1ZQV6SOG7jWMga%2BG7syAyMDE0MSx0deG6p24gdGjhu6kgMTUsbmfDoHkgMTYgdGjDoW5nIDExIG7Eg20gMjAxNApDaMO6bmcgdGEgY8OzIDE3IGtow6FjaCB2w6AgNDQ1IHRow6BuaCB2acOqbiB0cuG7sWMgdHV54bq%2FbiBkZAIHD2QWAmYPZBYCZg88KwAJAgAPFgIeDl8hVXNlVmlld1N0YXRlZ2QGD2QQFgFmFgE8KwAMAQAWAh4IU2VsZWN0ZWRnZGQYBwVCY3RsMDAkTWFpbkNvbnRlbnQkVGjDtG5nIGLDoW8geMOpdCBuaOG6rW4gxJHhu5Mgw6FuIHThu5F0IG5naGnhu4dwDxQrAAdkZmZmZmZoZAUyY3RsMDAkTWFpbkNvbnRlbnQkVGjDtG5nIGLDoW8geOG7rSBsw70gaOG7jWMgdOG6rXAPFCsAB2RmZmZmZmhkBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WCAUXY3RsMDAkY0xvZ0luMSRidF9jTG9nSW4FGGN0bDAwJGNUb3BNZW51MSRVc2VyTWVudQU0Y3RsMDAkTWFpbkNvbnRlbnQkVGjDtG5nIGLDoW8gY%2BG7p2EgYmFuIFF14bqjbiB0cuG7iwU0Y3RsMDAkTWFpbkNvbnRlbnQkVGjDtG5nIGLDoW8gxJHEg25nIGvDvSBo4buNYyB04bqtcAVCY3RsMDAkTWFpbkNvbnRlbnQkVGjDtG5nIGLDoW8geMOpdCBuaOG6rW4gxJHhu5Mgw6FuIHThu5F0IG5naGnhu4dwBTFjdGwwMCRNYWluQ29udGVudCRUaMO0bmcgYsOhbyB4w6l0IHThu5F0IG5naGnhu4dwBTJjdGwwMCRNYWluQ29udGVudCRUaMO0bmcgYsOhbyB44butIGzDvSBo4buNYyB04bqtcAUXY3RsMDAkTWFpbkNvbnRlbnQkY3RsMDYFMWN0bDAwJE1haW5Db250ZW50JFRow7RuZyBiw6FvIHjDqXQgdOG7kXQgbmdoaeG7h3APFCsAB2RmZmZmZmhkBTRjdGwwMCRNYWluQ29udGVudCRUaMO0bmcgYsOhbyDEkcSDbmcga8O9IGjhu41jIHThuq1wDxQrAAdkZmZmZmZoZAUXY3RsMDAkTWFpbkNvbnRlbnQkY3RsMDYPFCsAB2RmZmZmZmhkBTRjdGwwMCRNYWluQ29udGVudCRUaMO0bmcgYsOhbyBj4bunYSBiYW4gUXXhuqNuIHRy4buLDxQrAAdkZmZmZmZoZOIGHS%2BLUny89AQzw9o5hA%2F58aSXJEYwVzP8V1JEWeL%2F");
            login_post_data_list.Add("ctl00%24cLogIn1%24tb_cLogIn_User", username);
            login_post_data_list.Add("ctl00%24cLogIn1%24tb_cLogIn_Pass", password);
            String login_post_data = String.Empty;
            foreach (var field in login_post_data_list)
            {
                login_post_data += field.Key + '=' + field.Value + '&';
            }
            login_post_data.TrimEnd('&');
            return login_post_data;
        }

        // Tạo ra dữ liệu cho nội dung gói POST, phục vụ cho việc kiểm tra môn học
        private static String GenerateSubjectsPostData(String subjectsName)
        {
            Dictionary<string, string> subjects_post_data_list = new Dictionary<string, string>();

            if (subjectsName.Length == 6)
            {
                subjects_post_data_list.Add("__VIEWSTATE", "%2FwEPDwULLTE4NDAxMTU3NDQPZBYCZg9kFgICAw9kFgYCBQ9kFgJmDzwrAAQBAA8WAh4FVmFsdWUFekjhu41jIGvhu7MgMjAxNDEsdHXhuqduIHRo4bupIDE0LG5nw6B5IDE1IHRow6FuZyAxMSBuxINtIDIwMTQKQ2jDum5nIHRhIGPDsyAyMiBraMOhY2ggdsOgIDY0MyB0aMOgbmggdmnDqm4gdHLhu7FjIHR1eeG6v24gZGQCBw9kFgJmD2QWAmYPPCsACQIADxYCHg5fIVVzZVZpZXdTdGF0ZWdkBg9kEBYBAgEWATwrAAwBAQ9kEBYBZhYBPCsADAEAFgIeCFNlbGVjdGVkZ2RkZAILD2QWBAIHDxQrAAYPFgIeD0RhdGFTb3VyY2VCb3VuZGdkZGQ8KwAJAQgUKwAEFgQeEkVuYWJsZUNhbGxiYWNrTW9kZWgeJ0VuYWJsZVN5bmNocm9uaXphdGlvbk9uUGVyZm9ybUNhbGxiYWNrIGg8KwAEAQI8KwANAgAWAh4MSW1hZ2VTcGFjaW5nGwAAAAAAABBAAQAAAAwUKwABFgIeC1BhZGRpbmdMZWZ0GwAAAAAAABBAAQAAAA8WAh4KSXNTYXZlZEFsbGcPFCsAFxQrAAEWCB4EVGV4dAUDQWxsHwAFA0FsbB4ISW1hZ2VVcmxlHg5SdW50aW1lQ3JlYXRlZGcUKwABFggfCQUdS2hvYSBHacOhbyBk4bulYyB0aOG7gyBjaOG6pXQfAAUFQkdEVEMfCmUfC2cUKwABFggfCQUPVmnhu4duIEPGoSBraMOtHwAFA0tDSx8KZR8LZxQrAAEWCB8JBS1WaeG7h24gROG7h3QgbWF5IC0gRGEgZ2nhuqd5IHbDoCBUaOG7nWkgdHJhbmcfAAUIS0NORE1WVFQfCmUfC2cUKwABFggfCQUyVmnhu4duIEPDtG5nIG5naOG7hyBUaMO0bmcgdGluIHbDoCBUcnV54buBbiB0aMO0bmcfAAUFS0NOVFQfCmUfC2cUKwABFggfCQUeVmnhu4duIEvhu7kgdGh14bqtdCBIb8OhIGjhu41jHwAFBUtDTkhIHwplHwtnFCsAARYIHwkFDlZp4buHbiDEkGnhu4duHwAFAktEHwplHwtnFCsAARYIHwkFI1Zp4buHbiDEkGnhu4duIHThu60gLSBWaeG7hW4gdGjDtG5nHwAFBUtEVFZUHwplHwtnFCsAARYIHwkFHktob2EgR2nDoW8gZOG7pWMgUXXhu5FjIHBow7JuZx8ABQVLR0RRUB8KZR8LZxQrAAEWCB8JBR1WaeG7h24gS2luaCB04bq%2FICYgUXXhuqNuIGzDvR8ABQZLS1RWUUwfCmUfC2cUKwABFggfCQUvVmnhu4duIEtob2EgaOG7jWMgdsOgIEvhu7kgdGh14bqtdCBW4bqtdCBsaeG7h3UfAAUIS0tIVkNOVkwfCmUfC2cUKwABFggfCQUcS2hvYSBMw70gbHXhuq1uIGNow61uaCB0cuG7ix8ABQNLTUwfCmUfC2cUKwABFggfCQUUVmnhu4duIE5nb%2BG6oWkgbmfhu68fAAUDS05OHwplHwtnFCsAARYIHwkFHlZp4buHbiBTxrAgcGjhuqFtIEvhu7kgdGh14bqtdB8ABQVLU1BLVB8KZR8LZxQrAAEWCB8JBSdWaeG7h24gVG%2FDoW4g4bupbmcgZOG7pW5nIHbDoCBUaW4gaOG7jWMfAAUES1RURB8KZR8LZxQrAAEWCB8JBR9QaMOybmcgxJDDoG8gdOG6oW8gxJDhuqFpIGjhu41jHwAFBVBEVERIHwplHwtnFCsAARYGHwAFA1ZDSx8KZR8LZxQrAAEWCB8JBR1WaeG7h24gQ8ahIGtow60gxJDhu5luZyBs4buxYx8ABQVWQ0tETB8KZR8LZxQrAAEWCB8JBT1WaeG7h24gQ8O0bmcgbmdo4buHIFNpbmggaOG7jWMgdsOgIGPDtG5nIG5naOG7hyBUaOG7sWMgcGjhuqltHwAFCFZDTlNIVlRQHwplHwtnFCsAARYIHwkFPFZp4buHbiBL4bu5IHRodeG6rXQgSOG6oXQgbmjDom4gdsOgIFbhuq10IGzDvSBNw7RpIHRyxrDhu51uZx8ABQpWS1RITlZWTE1UHwplHwtnFCsAARYIHwkFMVZp4buHbiBLaG9hIGjhu41jIHbDoCBDw7RuZyBuZ2jhu4cgTcO0aSB0csaw4budbmcfAAUIVktIVkNOTVQfCmUfC2cUKwABFggfCQUxVmnhu4duIEtob2EgaOG7jWMgdsOgIEPDtG5nIG5naOG7hyBOaGnhu4d0IEzhuqFuaB8ABQhWS0hWQ05OTB8KZR8LZxQrAAEWCB8JBR1WaeG7h24gVuG6rXQgbMO9IGvhu7kgdGh14bqtdB8ABQVWVkxLVB8KZR8LZ2RkZGRkAg0PPCsAGAMADxYCHwNnZA8UKwABFgIeHUFsbG93T25seU9uZU1hc3RlclJvd0V4cGFuZGVkZxU8KwAGAQUUKwACZGRkGAEFHl9fQ29udHJvbHNSZXF1aXJlUG9zdEJhY2tLZXlfXxYEBRdjdGwwMCRjTG9nSW4xJGJ0X2NMb2dJbgUYY3RsMDAkY1RvcE1lbnUxJFVzZXJNZW51BSBjdGwwMCRNYWluQ29udGVudCRjYkFjYWRlbWljJERERAUfY3RsMDAkTWFpbkNvbnRlbnQkZ3ZDb3Vyc2VzR3JpZM2u7DHjYLAKkPZP%2FFWEiG3BhGBVhY5Xc7BxBvbQWZlo");
                subjects_post_data_list.Add("__CALLBACKID", "ctl00%24MainContent%24gvCoursesGrid");
                subjects_post_data_list.Add("__CALLBACKPARAM", "c0%3AKV%7C187%3B%5B'MI1010'%2C'MI1030'%2C'IT1010'%2C'FL1010'%2C'ME2010'%2C'SSH1030'%2C'PE1010'%2C'MI1020'%2C'MI1040'%2C'PH1010'%2C'FL1020'%2C'ME2020'%2C'SSH1010'%2C'PE1020'%2C'MIL1010'%2C'SSH1020'%2C'SSH1040'%2C'SSH1050'%2C'FL2010'%2C'PE1030'%5D%3BGB%7C47%3B11%7CAPPLYFILTER30%7CContains(%5BCourseID%5D%2C%20'" + subjectsName + "')%3B");
            }

            if (subjectsName.Length == 7)
            {
                subjects_post_data_list.Add("__VIEWSTATE", "%2FwEPDwULLTE4NDAxMTU3NDQPZBYCZg9kFgICAw9kFgYCBQ9kFgJmDzwrAAQBAA8WAh4FVmFsdWUFeUjhu41jIGvhu7MgMjAxNDEsdHXhuqduIHRo4bupIDE0LG5nw6B5IDE1IHRow6FuZyAxMSBuxINtIDIwMTQKQ2jDum5nIHRhIGPDsyA1IGtow6FjaCB2w6AgNjQzIHRow6BuaCB2acOqbiB0cuG7sWMgdHV54bq%2FbiBkZAIHD2QWAmYPZBYCZg88KwAJAgAPFgIeDl8hVXNlVmlld1N0YXRlZ2QGD2QQFgECARYBPCsADAEBD2QQFgFmFgE8KwAMAQAWAh4IU2VsZWN0ZWRnZGRkAgsPZBYEAgcPFCsABg8WAh4PRGF0YVNvdXJjZUJvdW5kZ2RkZDwrAAkBCBQrAAQWBB4SRW5hYmxlQ2FsbGJhY2tNb2RlaB4nRW5hYmxlU3luY2hyb25pemF0aW9uT25QZXJmb3JtQ2FsbGJhY2sgaDwrAAQBAjwrAA0CABYCHgxJbWFnZVNwYWNpbmcbAAAAAAAAEEABAAAADBQrAAEWAh4LUGFkZGluZ0xlZnQbAAAAAAAAEEABAAAADxYCHgpJc1NhdmVkQWxsZw8UKwAXFCsAARYIHgRUZXh0BQNBbGwfAAUDQWxsHghJbWFnZVVybGUeDlJ1bnRpbWVDcmVhdGVkZxQrAAEWCB8JBR1LaG9hIEdpw6FvIGThu6VjIHRo4buDIGNo4bqldB8ABQVCR0RUQx8KZR8LZxQrAAEWCB8JBQ9WaeG7h24gQ8ahIGtow60fAAUDS0NLHwplHwtnFCsAARYIHwkFLVZp4buHbiBE4buHdCBtYXkgLSBEYSBnaeG6p3kgdsOgIFRo4budaSB0cmFuZx8ABQhLQ05ETVZUVB8KZR8LZxQrAAEWCB8JBTJWaeG7h24gQ8O0bmcgbmdo4buHIFRow7RuZyB0aW4gdsOgIFRydXnhu4FuIHRow7RuZx8ABQVLQ05UVB8KZR8LZxQrAAEWCB8JBR5WaeG7h24gS%2BG7uSB0aHXhuq10IEhvw6EgaOG7jWMfAAUFS0NOSEgfCmUfC2cUKwABFggfCQUOVmnhu4duIMSQaeG7h24fAAUCS0QfCmUfC2cUKwABFggfCQUjVmnhu4duIMSQaeG7h24gdOG7rSAtIFZp4buFbiB0aMO0bmcfAAUFS0RUVlQfCmUfC2cUKwABFggfCQUeS2hvYSBHacOhbyBk4bulYyBRdeG7kWMgcGjDsm5nHwAFBUtHRFFQHwplHwtnFCsAARYIHwkFHVZp4buHbiBLaW5oIHThur8gJiBRdeG6o24gbMO9HwAFBktLVFZRTB8KZR8LZxQrAAEWCB8JBS9WaeG7h24gS2hvYSBo4buNYyB2w6AgS%2BG7uSB0aHXhuq10IFbhuq10IGxp4buHdR8ABQhLS0hWQ05WTB8KZR8LZxQrAAEWCB8JBRxLaG9hIEzDvSBsdeG6rW4gY2jDrW5oIHRy4buLHwAFA0tNTB8KZR8LZxQrAAEWCB8JBRRWaeG7h24gTmdv4bqhaSBuZ%2BG7rx8ABQNLTk4fCmUfC2cUKwABFggfCQUeVmnhu4duIFPGsCBwaOG6oW0gS%2BG7uSB0aHXhuq10HwAFBUtTUEtUHwplHwtnFCsAARYIHwkFJ1Zp4buHbiBUb8OhbiDhu6luZyBk4bulbmcgdsOgIFRpbiBo4buNYx8ABQRLVFREHwplHwtnFCsAARYIHwkFH1Bow7JuZyDEkMOgbyB04bqhbyDEkOG6oWkgaOG7jWMfAAUFUERUREgfCmUfC2cUKwABFgYfAAUDVkNLHwplHwtnFCsAARYIHwkFHVZp4buHbiBDxqEga2jDrSDEkOG7mW5nIGzhu7FjHwAFBVZDS0RMHwplHwtnFCsAARYIHwkFPVZp4buHbiBDw7RuZyBuZ2jhu4cgU2luaCBo4buNYyB2w6AgY8O0bmcgbmdo4buHIFRo4buxYyBwaOG6qW0fAAUIVkNOU0hWVFAfCmUfC2cUKwABFggfCQU8Vmnhu4duIEvhu7kgdGh14bqtdCBI4bqhdCBuaMOibiB2w6AgVuG6rXQgbMO9IE3DtGkgdHLGsOG7nW5nHwAFClZLVEhOVlZMTVQfCmUfC2cUKwABFggfCQUxVmnhu4duIEtob2EgaOG7jWMgdsOgIEPDtG5nIG5naOG7hyBNw7RpIHRyxrDhu51uZx8ABQhWS0hWQ05NVB8KZR8LZxQrAAEWCB8JBTFWaeG7h24gS2hvYSBo4buNYyB2w6AgQ8O0bmcgbmdo4buHIE5oaeG7h3QgTOG6oW5oHwAFCFZLSFZDTk5MHwplHwtnFCsAARYIHwkFHVZp4buHbiBW4bqtdCBsw70ga%2BG7uSB0aHXhuq10HwAFBVZWTEtUHwplHwtnZGRkZGQCDQ88KwAYAwAPFgIfA2dkDxQrAAEWAh4dQWxsb3dPbmx5T25lTWFzdGVyUm93RXhwYW5kZWRnFTwrAAYBBRQrAAJkZGQYAQUeX19Db250cm9sc1JlcXVpcmVQb3N0QmFja0tleV9fFgQFF2N0bDAwJGNMb2dJbjEkYnRfY0xvZ0luBRhjdGwwMCRjVG9wTWVudTEkVXNlck1lbnUFIGN0bDAwJE1haW5Db250ZW50JGNiQWNhZGVtaWMkREREBR9jdGwwMCRNYWluQ29udGVudCRndkNvdXJzZXNHcmlkoC8skGgjuH8RzK0NXlDXs4G0Guq0BznHWAB7w9DqGY8%3D");
                subjects_post_data_list.Add("__CALLBACKID", "ctl00%24MainContent%24gvCoursesGrid");
                subjects_post_data_list.Add("__CALLBACKPARAM", "c0%3AKV%7C187%3B%5B'MI1010'%2C'MI1030'%2C'IT1010'%2C'FL1010'%2C'ME2010'%2C'SSH1030'%2C'PE1010'%2C'MI1020'%2C'MI1040'%2C'PH1010'%2C'FL1020'%2C'ME2020'%2C'SSH1010'%2C'PE1020'%2C'MIL1010'%2C'SSH1020'%2C'SSH1040'%2C'SSH1050'%2C'FL2010'%2C'PE1030'%5D%3BGB%7C48%3B11%7CAPPLYFILTER31%7CContains(%5BCourseID%5D%2C%20'" + subjectsName + "')%3B");
            }
            
            String subjects_post_data = String.Empty;
            foreach (var field in subjects_post_data_list)
            {
                subjects_post_data += field.Key + '=' + field.Value + '&';
            }
            subjects_post_data.TrimEnd('&');
            return subjects_post_data;
        }

        // Kiểm tra xem trang web có đúng tìm kiếm hay không
        private static Boolean VerifyWebContent(String page, String expected_string)
        {
            if (expected_string == null) return true;

            if (page.IndexOf(expected_string) < 0)
            {
                return false;
            }
            return true;
        }
        
        // Lấy nội dung trang web về bằng việc gửi POST
        private static String Send_POST(String URL, String referer, String content)
        {
            try
            {
                HttpWebRequest web_request = (HttpWebRequest)WebRequest.Create(URLbase + URL);
                web_request.Method = "POST";
                web_request.ProtocolVersion = HttpVersion.Version11;
                web_request.CookieContainer = _cookies;
                if (referer != null) web_request.Referer = URLbase + referer;
                web_request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                byte[] byteArray = Encoding.UTF8.GetBytes(content);
                web_request.ContentLength = byteArray.Length;
                web_request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                // Đưa webrequest vào 1 steam
                Stream dataStream = web_request.GetRequestStream();
                // Gửi phân thân POST tới SIS
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Đóng Stream object.
                dataStream.Close();

                /// - Gửi HTTP Request và lấy HTTP Response về. Response này đã có đầy đủ thông tin về phần header, nhưng chưa thực sự lấy nội dung về.
                HttpWebResponse myResponse = (HttpWebResponse)web_request.GetResponse();
                /// - Đưa HTTP Response vào một Stream, stream sẽ được dùng để đọc phần thân của Response từ server.
                StreamReader Reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                /// - Đọc toàn bộ nội dung, phần thân của HTTP Response vào biến
                String Html = Reader.ReadToEnd();
                Reader.Close();
                return Html;
            }
            catch (SystemException e)
            {
                throw e;
            }
        }

        // Lấy nội dung trang web về bằng việc gửi GET 
        private static String Send_GET(String URL, String referer)
        {
            try
            {
                HttpWebRequest web_request = (HttpWebRequest)WebRequest.Create(URLbase + URL);
                web_request.Method = "GET";
                web_request.AllowAutoRedirect = false;
                web_request.ProtocolVersion = HttpVersion.Version11;
                web_request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                web_request.Headers.Add("Accept-Language: vi-vn,vi;q=0.8,en-us;q=0.5,en;q=0.3");
                web_request.KeepAlive = false;
                web_request.Headers.Add("Keep-Alive: 115");
                web_request.PreAuthenticate = false;
                web_request.Pipelined = true;
                if (referer != null) web_request.Referer = URLbase + referer;
                web_request.CookieContainer = _cookies;
                web_request.ContentType = "text/html";

                /// - Gửi HTTP Request và lấy HTTP Response về. Response này đã có đầy đủ thông tin về phần header, nhưng chưa thực sự lấy nội dung về.
                HttpWebResponse myResponse = (HttpWebResponse)web_request.GetResponse();
                /// - Đưa HTTP Response vào một Stream, stream sẽ được dùng để đọc phần thân của Response từ server.
                StreamReader Reader = new StreamReader(myResponse.GetResponseStream(), Encoding.Default);
                /// - Đọc toàn bộ nội dung, phần thân của HTTP Response vào biến
                String Html = Reader.ReadToEnd();
                /// - Đóng các kết nối lại.
                Reader.Close();
                myResponse.Close();
                return Html;
            }
            catch (SystemException e)
            {
                throw e;
            }
        }

        // Crawler dữ liệu về môn học
        private static void CrawlerSubjects(String content)
        {
            try
            {
                // khởi tạo HtmlAgilityPack
                HtmlAgilityPack.HtmlDocument page = new HtmlAgilityPack.HtmlDocument();
                // load nội dung html lấy được ở trên
                page.LoadHtml(content);
                // sử dụng xpath truy cập vào các node
                HtmlAgilityPack.HtmlNodeCollection newsitem = page.DocumentNode.SelectNodes("//*[@id=\"MainContent_gvCoursesGrid_DXDataRow0\"]");
                // hiển thị những nội dung trong node đó
                foreach (var item in newsitem)
                {
                    Console.WriteLine("- Mã số môn học: " + item.SelectSingleNode("//*[@id=\"MainContent_gvCoursesGrid_DXDataRow0\"]/td[2]").InnerText);
                    Console.WriteLine("- Tên môn học: " + item.SelectSingleNode("//*[@id=\"MainContent_gvCoursesGrid_DXDataRow0\"]/td[3]").InnerText);
                    Console.WriteLine("- Thời lượng: " + item.SelectSingleNode("//*[@id=\"MainContent_gvCoursesGrid_DXDataRow0\"]/td[4]").InnerText);
                    Console.WriteLine("- Số tín chỉ: " + item.SelectSingleNode("//*[@id=\"MainContent_gvCoursesGrid_DXDataRow0\"]/td[5]").InnerText);
                    Console.WriteLine("- Tín chỉ học phí: " + item.SelectSingleNode("//*[@id=\"MainContent_gvCoursesGrid_DXDataRow0\"]/td[6]").InnerText);
                    Console.WriteLine("- Hệ sô: " + item.SelectSingleNode("//*[@id=\"MainContent_gvCoursesGrid_DXDataRow0\"]/td[7]").InnerText);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Crawler dữ liệu về thông tin người dùng
        private static void CrawlerInfo(String content)
        {
            try
            {
            
                // khởi tạo HtmlAgilityPack
                HtmlAgilityPack.HtmlDocument pages = new HtmlAgilityPack.HtmlDocument();
                // load nội dung html lấy được ở trên
                pages.LoadHtml(content);
                // sử dụng xpath truy cập vào các node

                HtmlAgilityPack.HtmlNodeCollection newsites = pages.DocumentNode.SelectNodes("//*[@id=\"mainTextBody\"]/div[2]/table/tr/td[1]/div");
                // hiển thị những nội dung trong node đó
                foreach (var item in newsites)
                {
                    Console.WriteLine("\n- Mã số sinh viên: " + item.SelectSingleNode("//*[@id=\"mainTextBody\"]/div[2]/table/tr/td[1]/div/p[1]/b").InnerText);
                    Console.WriteLine("- Họ tên sinh viên: " + item.SelectSingleNode("//*[@id=\"mainTextBody\"]/div[2]/table/tr/td[1]/div/p[2]/b").InnerText);
                    Console.WriteLine("- Ngày sinh:" + item.SelectSingleNode("//*[@id=\"mainTextBody\"]/div[2]/table/tr/td[1]/div/p[3]/b").InnerText);
                    Console.WriteLine("- Lớp: " + item.SelectSingleNode("//*[@id=\"mainTextBody\"]/div[2]/table/tr/td[1]/div/p[4]/b").InnerText);
                    Console.WriteLine("- Chương trình: " + item.SelectSingleNode("//*[@id=\"mainTextBody\"]/div[2]/table/tr/td[1]/div/p[5]/b").InnerText);
                    Console.WriteLine("- Hệ học: " + item.SelectSingleNode("//*[@id=\"mainTextBody\"]/div[2]/table/tr/td[1]/div/p[6]/b").InnerText);
                    Console.WriteLine("- Tình trạng: " + item.SelectSingleNode("//*[@id=\"mainTextBody\"]/div[2]/table/tr/td[1]/div/p[7]/b").InnerText);
                }
            
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
