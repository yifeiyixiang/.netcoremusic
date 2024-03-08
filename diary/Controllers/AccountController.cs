using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using diary.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace diary.Views.Account
{

    public class AccountController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        public AccountController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public static SqlLiteHelper sqlhelper = new SqlLiteHelper();
        // GET: AccountController 
        public ActionResult Login()
        {
            var username = HttpContext.Request.Cookies["UserName"];
            ViewBag.name = username;
            string cookie = Request.Cookies["UserName"];
         
            return View();
        }
        public ActionResult index()
        {
            var username = HttpContext.Request.Cookies["UserName"];
            ViewBag.name = username;
            string cookie = Request.Cookies["UserName"];
            
            return View();
        }
        [HttpPost]
        public ActionResult Login(string account, string pwd)
        {
            string sql = string.Format("select * from SysUser where UserName='{0}' and Password='{1}'", account, pwd);
            var table = sqlhelper.GetDataTable(sql);
            if (table.Rows.Count > 0)
            {
                // 保存用户信息到 Session
                var name = table.Rows[0][1].ToString();
                var id = Convert.ToInt32(table.Rows[0][0]);
                HttpContext.Session.SetString("UserName", name);
                HttpContext.Session.SetInt32("UserId", id);  //在Session中保存用户信息
                HttpContext.Response.Cookies.Append("UserName", name); 
                CookieOptions options = new CookieOptions();
                // 设置过期时间
                options.Expires = DateTime.Now.AddDays(7);
                HttpContext.Response.Cookies.Append("setCookieExpires", "CookieValueExpires", options);
            }  
            else
            {
                TempData["msg"] = "账号或密码错误，登录失败";
            }
            return RedirectToAction("Login");
        }
        public ActionResult LogOut()
        {
            HttpContext.Response.Cookies.Delete("UserName");
            return RedirectToAction("Login");
        }
        // GET: AccountController/Details/5
        public ActionResult Details(int id)
        { 
            return View();
        }
        //获取音乐列表 通过前端歌单名字获取 音乐 歌单名存登陆里面还是
        public string GetMusicsrc(string album)
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "music", album);
            // 检查该专辑文件夹是否存在
            if (Directory.Exists(folderPath2))
            {
                var musicFiles = Directory.GetFiles(folderPath2, "*.mp3");
                string[] extractedPaths = new string[musicFiles.Length];

                for (int i = 0; i < musicFiles.Length; i++)
                {
                    string filePath = musicFiles[i];
                    int index = filePath.IndexOf($"{Path.DirectorySeparatorChar}music{Path.DirectorySeparatorChar}");
                    if (index >= 0)
                    {
                        extractedPaths[i] = filePath.Substring(index);
                    }
                    else
                    {
                        extractedPaths[i] = "";
                    }
                }


                string json = JsonSerializer.Serialize(extractedPaths);
                //string json = JsonSerializer.Serialize(folderPath2 + "@" + musicFiles[0] + "@" + extractedPaths);
                return json;
            }
            else
            {
                // 如果专辑文件夹不存在，返回空数组或适当的错误信息
                return "[]";
            }
            }

        public string Getalbum(string list)
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "music"); 
            var subFolders = Directory.GetDirectories(folderPath);
            // 创建一个包含子文件夹名称的动态对象列表
            var folderList = new List<dynamic>();
            foreach (var subFolder in subFolders)
            {
                var folderName = new DirectoryInfo(subFolder).Name;
                folderList.Add(new { Name = folderName });
            }

            // 将动态对象列表转换为 JSON 字符串
            var json = JsonSerializer.Serialize(folderList, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }); 

             
            //string json = JsonSerializer.Serialize(folderPath2 + "@" + musicFiles[0] + "@" + extractedPaths);
            return json;
        }
        public string GetMusicname(string album)
        {
            string folderPath2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "music", album);
            // 检查该专辑文件夹是否存在
            if (Directory.Exists(folderPath2))
            {
                //string folderPath = path + "music"; // 替换为实际的音乐文件夹路径
                var musicFiles = Directory.GetFiles(folderPath2, "*.mp3").Select(file => Path.GetFileNameWithoutExtension(file)); // 获取文件名（不包含路径和后缀名）  
            string json = JsonSerializer.Serialize(musicFiles);
            return json;
            }
            else
            {
                // 如果专辑文件夹不存在，返回空数组或适当的错误信息
                return "[]";
            }
        }
        public IActionResult GetMusic(int currIndex)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = path + "music"; // 替换为实际的音乐文件夹路径
            var musicFiles = Directory.GetFiles(folderPath, "*.mp3"); // 替换为音乐文件的扩展名或通配符
            // 根据currIndex获取对应音乐的URL
            string[] musicUrls = musicFiles;
            string musicUrl = musicUrls[currIndex];

            // 根据音乐URL获取音乐文件的绝对路径
            string filePath = Path.Combine(_environment.WebRootPath, musicUrl);

            // 检查文件是否存在
            if (System.IO.File.Exists(filePath))
            {
                // 返回音乐文件给前端
                return File(filePath, "audio/mp3");
            }
            else
            {
                // 文件不存在，返回404状态码
                return NotFound();
            }
        }
        // GET: AccountController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AccountController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AccountController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AccountController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
