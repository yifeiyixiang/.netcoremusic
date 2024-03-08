using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using diary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;

namespace diary.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public SqlLiteHelper sqlhelper = new SqlLiteHelper(); 
        string recentlysql = "select * from diary  order by date desc limit 1"; 
        
        [HttpPost]
      
      
        private List<Dictionary<string, object>> ConvertDataTableToList(DataTable table)
        {
            List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                Dictionary<string, object> rowData = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    rowData[col.ColumnName] = row[col];
                }

                dataList.Add(rowData);
            }

            return dataList;
        }
        public IActionResult Privacy(string key)
        {
             
            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.GetCultureInfo("zh-CN"));              // 2008-9-4 20:02:10

            var date2 = DateTime.Now.ToLongDateString().ToString();    // 2008年9月4日

            var week = DateTime.Now.DayOfWeek.ToString();  //获取星期   // Thursday 
            var chineseWeek = ConvertToChineseWeek(week);
            DateTime.Now.Year.ToString();  //获取年份   // 2008
            DateTime.Now.Month.ToString();  //获取月份   // 9 
            
            ViewBag.date = date;
            ViewBag.week = chineseWeek;
            ViewBag.dbstr = sqlhelper.mydbPath;
            bool isUserValid = HttpContext.Request.Cookies["UserName"] != null;
            ViewBag.IsUserValid = isUserValid;
            ViewBag.IsUserValidjs = isUserValid.ToString().ToLower();
           
            return View(); 
        }
        
     
        [DisableRequestSizeLimit] //不限制大小
        public IActionResult UploadFile(List<IFormFile>  files, string playlist)
        {
            
            var username = HttpContext.Request.Cookies["UserName"];
            ViewBag.name = username;
            string cookie = Request.Cookies["UserName"];
            if (username != null)
            {
                foreach (var file in files)
                {  
                if (file != null && file.Length > 0)
                {
                    // 获取上传文件的文件名
                    var fileName = Path.GetFileName(file.FileName);
                    // 验证文件后缀名是否为.db
                    if (Path.GetExtension(fileName) != ".db")
                    {
                        // 如果文件后缀名不是.db，返回错误信息 上传mp3文件
                        ModelState.AddModelError("file", "只能上传.db文件");
                            // 验证文件后缀名是否为.db mp3
                            if (Path.GetExtension(fileName) != ".mp3")
                            {
                                // 如果文件后缀名不是.db，返回错误信息 都不是就跳
                                ModelState.AddModelError("file", "只能上传.mp3文件");
                                return RedirectToAction("Privacy");
                            }
                            else
                            {
                                var fileNamemp3 = fileName;
                                // 获取上传文件的保存路径
                                //var filePath = Path.Combine(Directory.GetCurrentDirectory(), sqlhelper.dirdbPath, fileName);
                                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "music", playlist);
                                if (Directory.Exists(folderPath))
                                {
                                    string filePath2 = Path.Combine(folderPath, fileNamemp3);
                                    if (System.IO.File.Exists(filePath2))//文件是否存在
                                    {
                                        // 如果文件后缀名不是.db，返回错误信息// 文件夹存在，执行相应的逻辑 不保存
                                        ModelState.AddModelError("file", " mp3文件存在"); 
                                    }
                                    else
                                    { 
                                        // 不存在就 保存上传的文件
                                        using (var stream = new FileStream(filePath2, FileMode.Create))
                                        {
                                            file.CopyTo(stream);
                                        }
                                    }
                                   
                                    //mp3上传完跳 不了,要继续循环
                                }
                                else
                                {
                                    // 文件夹不存在，执行相应的逻辑
                                }


                            }
                        }
                        else {
                            // 获取上传文件的保存路径
                            //var filePath = Path.Combine(Directory.GetCurrentDirectory(), sqlhelper.dirdbPath, fileName);
                            var filePath = sqlhelper.mydbPath;
                            // 删除之前的数据库文件
                            if (System.IO.File.Exists(filePath))
                            {
                                // 获取旧文件的文件名
                                var oldFileName = Path.GetFileName(filePath);
                                // 获取当前日期作为文件名前缀
                                string prefix = DateTime.Now.ToString("yyyyMMdd");
                                // 构造新的文件名
                                string newFileName = prefix + "_" + oldFileName;
                                // 构造旧文件的完整路径 diary.db
                                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), sqlhelper.dirdbPath, oldFileName);
                                // 构造新文件的完整路径
                                var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), sqlhelper.dirdbPath, newFileName);
                                // 删除之前 当天的数据库文件
                                if (System.IO.File.Exists(newFilePath))
                                {
                                    System.IO.File.Delete(newFilePath);
                                }
                                // 将旧文件重命名为新文件名  20230115_diary.db
                                System.IO.File.Move(oldFilePath, newFilePath);
                                //System.IO.File.Delete(filePath);
                                // 删除前一天的文件名的文件
                                var previousDayPrefix = DateTime.Now.AddDays(-2).ToString("yyyyMMdd");
                                var previousDayFileName = previousDayPrefix + "_" + oldFileName;
                                var previousDayFilePath = Path.Combine(Directory.GetCurrentDirectory(), sqlhelper.dirdbPath, previousDayFileName);
                                // 删除之前 前一天的数据库文件
                                if (System.IO.File.Exists(previousDayFilePath))
                                {
                                    System.IO.File.Delete(previousDayFilePath);
                                }

                            }
                            // 保存上传的文件
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            return RedirectToAction("Privacy");
                        } 
                        // 执行其他操作，例如更新数据库等

                    }  
                }
            }
            return RedirectToAction("Privacy");
        }
  
        
   

        public string ConvertToChineseWeek(string week)
        {
            switch (week)
            {
                case "Monday":
                    return ChineseWeek.星期一.ToString();
                case "Tuesday":
                    return ChineseWeek.星期二.ToString();
                case "Wednesday":
                    return ChineseWeek.星期三.ToString();
                case "Thursday":
                    return ChineseWeek.星期四.ToString();
                case "Friday":
                    return ChineseWeek.星期五.ToString();
                case "Saturday":
                    return ChineseWeek.星期六.ToString();
                case "Sunday":
                    return ChineseWeek.星期天.ToString();
                default:
                    return string.Empty;
            }
        }

        enum ChineseWeek
        {
            星期一,
            星期二,
            星期三,
            星期四,
            星期五,
            星期六,
            星期天
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        

        //[EnableCors("AllowSpecificOrigin")]
        [HttpPost]
        public async Task<IActionResult> RuleUploadFile(IFormFile file, string playlist,string Name, int Number, int BufferSize, int Count, long Size, long Start, long End)
        {
            var username = HttpContext.Request.Cookies["UserName"];
            ViewBag.name = username;
            string cookie = Request.Cookies["UserName"];
            if (username != null)
            {
                 if (file != null && file.Length > 0)
                    {
                        // 获取上传文件的文件名 db文件没有用分片 前提是db文件不大于8mb 目前68kb 如果大于 就要考虑分片了
                        var fileName = Path.GetFileName(Name);
                        // 验证文件后缀名是否为.db
                        if (Path.GetExtension(fileName) != ".db")
                        {
                            // 如果文件后缀名不是.db，返回错误信息 上传mp3文件 并且使用分片上传
                            ModelState.AddModelError("file", "只能上传.db文件");
                            // 验证文件后缀名是否为.db mp3
                            if (Path.GetExtension(fileName) != ".mp3")
                            {
                                // 如果文件后缀名不是.db，返回错误信息 都不是就跳
                                ModelState.AddModelError("file", "只能上传.mp3文件");
                                return RedirectToAction("Privacy");
                            }
                            else
                            {
                            try
                            {
                                //string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload");//这个路径js不好获取
                                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "music", playlist);
                                string path = folderPath;
                                //获取文件上传边界
                                var files = Request.Form.Files;
                                var buffer = new byte[Size]; 
                                // 获取上传文件的文件名 
                                path = path + "//" + fileName.Split(".")[0].ToString() + "//";
                                if (!System.IO.Directory.Exists(path)) // 创建歌单文件夹 
                                {
                                    System.IO.Directory.CreateDirectory(path);
                                }
                                string filepath = path + "//" +Name + "^" + Number;
                                if (System.IO.File.Exists(filepath))//mp3分片文件是否存在
                                {
                                    // 如果文件后缀名不是.db，返回错误信息// 文件夹存在，执行相应的逻辑 不保存
                                    ModelState.AddModelError("file", " mp3分片文件存在");
                                }
                                else
                                {
                                    using (var stream = new FileStream(filepath, FileMode.Append))
                                    {
                                        await files[0].CopyToAsync(stream);
                                    }
                                }
                             
                                var filesList = Directory.GetFiles(Path.GetDirectoryName(path));

                                //当顺序号等于分片总数量 合并文件
                                if ((Number + 1) == Count || filesList.Length == Count)
                                {
                                    await MergeFile(file, Count, Name, folderPath);
                                    //await MergeFile2(file, Count, Name);
                                }
                                return this.Ok();

                            }
                            catch (Exception ex)
                            {
                                return BadRequest(ex.Message);
                            } 

                            }
                        }
                        else
                        {
                            // 获取上传文件的保存路径
                            //var filePath = Path.Combine(Directory.GetCurrentDirectory(), sqlhelper.dirdbPath, fileName);
                            var filePath = sqlhelper.mydbPath;
                            // 删除之前的数据库文件
                            if (System.IO.File.Exists(filePath))
                            {
                                // 获取旧文件的文件名
                                var oldFileName = Path.GetFileName(filePath);
                                // 获取当前日期作为文件名前缀
                                string prefix = DateTime.Now.ToString("yyyyMMdd");
                                // 构造新的文件名
                                string newFileName = prefix + "_" + oldFileName;
                                // 构造旧文件的完整路径 diary.db
                                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), sqlhelper.dirdbPath, oldFileName);
                                // 构造新文件的完整路径
                                var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), sqlhelper.dirdbPath, newFileName);
                                // 删除之前 当天的数据库文件
                                if (System.IO.File.Exists(newFilePath))
                                {
                                    System.IO.File.Delete(newFilePath);
                                }
                                // 将旧文件重命名为新文件名  20230115_diary.db
                                System.IO.File.Move(oldFilePath, newFilePath);
                                //System.IO.File.Delete(filePath);
                                // 删除前一天的文件名的文件
                                var previousDayPrefix = DateTime.Now.AddDays(-2).ToString("yyyyMMdd");
                                var previousDayFileName = previousDayPrefix + "_" + oldFileName;
                                var previousDayFilePath = Path.Combine(Directory.GetCurrentDirectory(), sqlhelper.dirdbPath, previousDayFileName);
                                // 删除之前 前一天的数据库文件
                                if (System.IO.File.Exists(previousDayFilePath))
                                {
                                    System.IO.File.Delete(previousDayFilePath);
                                }

                            }
                            // 保存上传的文件
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            return RedirectToAction("Privacy");
                        }
                        // 执行其他操作，例如更新数据库等

                    } 
            }
            return RedirectToAction("Privacy");
      

        }
        /// <summary>
        /// 合并文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task MergeFile(IFormFile file,int count,string name,  string folderPath)
        {
            string path = folderPath;
            var fileName = Path.GetFileName(name);
            path = path + "//" + fileName.Split(".")[0].ToString() + "//";
            //string baseFileName = path + fileName.Split("~")[0].ToString();
            //string baseFileName = folderPath  +"//" + fileName;
            string baseFileName = path + fileName;

            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            var filesList = Directory.GetFiles(Path.GetDirectoryName(path));
            if (filesList.Length != count)
            {
                return;
            }
            List<FileSort> lstFile = new List<FileSort>();
            foreach (var item in filesList)
            {
                lstFile.Add(new FileSort()
                {
                    Name = item,
                    NumBer = Convert.ToInt32(item.Substring(item.IndexOf('^') + 1))
                });
            }
            lstFile = lstFile.OrderBy(x => x.NumBer).ToList();
            using (var fileStream = new FileStream(baseFileName, FileMode.Create))
            {
                //foreach (var fileSort in filesList)
                //{
                //    using (FileStream fileChunk = new FileStream(fileSort, FileMode.Open))
                //    {
                //        await fileChunk.CopyToAsync(fileStream);
                //    }

                //}
                await Policy.Handle<IOException>()
                        .RetryForeverAsync()
                        .ExecuteAsync(async () =>
                        {
                            foreach (var fileSort in lstFile)
                            {
                                using (FileStream fileChunk = new FileStream(fileSort.Name, FileMode.Open))
                                {
                                    await fileChunk.CopyToAsync(fileStream);
                                }

                            }
                        }); 

            }
            try
            {
             var   newFilePath = Path.Combine(folderPath, Path.GetFileName(fileName)); // 更新目标文件路径
                System.IO.File.Copy(baseFileName, newFilePath);
                Console.WriteLine("文件移动成功！");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"文件移动失败: {ex.Message}");
            }
            //删除分片文件 和临时分片文件夹
            foreach (var dirfile in filesList)
            {
                System.IO.File.Delete(dirfile);
            }
            Directory.Delete(path, true); // 这里的 true 表示同时删除子目录和文件
        }
        private async Task MergeFile2(IFormFile file, int count, string name)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload");
            var fileName = name;
            path = path + "//" + fileName + "//";
            string baseFileName = path + fileName.Split("~")[0].ToString();
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            var filesList = Directory.GetFiles(Path.GetDirectoryName(path));
            if (filesList.Length != count)
            {
                return;
            }
            List<FileSort> lstFile = new List<FileSort>();
            foreach (var item in filesList)
            {
                lstFile.Add(new FileSort()
                {
                    Name = item,
                    NumBer = Convert.ToInt32(item.Substring(item.IndexOf('^') + 1))
                });
            }
            lstFile = lstFile.OrderBy(x => x.NumBer).ToList();
            using (var fileStream = new FileStream(baseFileName, FileMode.Create))
            {
                //foreach (var fileSort in filesList)
                //{
                //    using (FileStream fileChunk = new FileStream(fileSort, FileMode.Open))
                //    {
                //        await fileChunk.CopyToAsync(fileStream);
                //    }

                //}
                await Policy.Handle<IOException>()
                        .RetryForeverAsync()
                        .ExecuteAsync(async () =>
                        {
                            foreach (var fileSort in lstFile)
                            {
                                using (FileStream fileChunk = new FileStream(fileSort.Name, FileMode.Open))
                                {
                                    await fileChunk.CopyToAsync(fileStream);
                                }

                            }
                        });


            }
            //删除分片文件
            foreach (var dirfile in filesList)
            {
                System.IO.File.Delete(dirfile);
            }
        }
        public IActionResult Default()
        {
            return View();
        }
    }
}
