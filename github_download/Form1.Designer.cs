using System;
using System.Net;
using System.Windows.Forms;
using System.Net.Http;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Security.Policy;
using System.Diagnostics;
using System.Text.Json;

namespace github_download
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox textBox1;
        private TextBox textBox2;
        private Button downloadButton;
        private Button selectDirectoryButton;
        private FolderBrowserDialog folderBrowserDialog;
        private Label label1;
        private Label label2;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            folderBrowserDialog = new FolderBrowserDialog();
            label1 = new Label();
            textBox1 = new TextBox();
            selectDirectoryButton = new Button();
            label2 = new Label();
            textBox2 = new TextBox();
            downloadButton = new Button();
            SuspendLayout();
            // 
            // folderBrowserDialog
            // 
            string userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloadsFolder = Path.Combine(userProfileFolder, "Downloads");

            folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            folderBrowserDialog.SelectedPath = downloadsFolder;
            // 
            // label1
            // 
            label1.Location = new Point(30, 50);
            label1.Name = "label1";
            label1.Size = new Size(65, 20);
            label1.TabIndex = 0;
            label1.Text = "下载目录:";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(101, 47);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(259, 23);
            textBox1.TabIndex = 0;
            textBox1.Text = downloadsFolder;
            // 
            // selectDirectoryButton
            // 
            selectDirectoryButton.Location = new Point(366, 47);
            selectDirectoryButton.Name = "selectDirectoryButton";
            selectDirectoryButton.Size = new Size(85, 23);
            selectDirectoryButton.TabIndex = 1;
            selectDirectoryButton.Text = "选择目录";
            selectDirectoryButton.Click += SelectDirectoryButton_Click;
            // 
            // label2
            // 
            label2.Location = new Point(30, 108);
            label2.Name = "label2";
            label2.Size = new Size(65, 20);
            label2.TabIndex = 2;
            label2.Text = "下载地址:";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(101, 105);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(350, 23);
            textBox2.TabIndex = 1;
            // 
            // downloadButton
            // 
            downloadButton.Location = new Point(101, 160);
            downloadButton.Name = "downloadButton";
            downloadButton.Size = new Size(303, 33);
            downloadButton.TabIndex = 2;
            downloadButton.Text = "下载";
            downloadButton.Click += DownloadButton_Click;

            // Set form properties
            FormBorderStyle = FormBorderStyle.FixedSingle; // Set form border style to fixed
            MaximizeBox = false; // Disable maximize box
            StartPosition = FormStartPosition.CenterScreen; // Center the form on the screen

            // Set focus to the first textbox
            textBox1.Focus();
            textBox1.Select(textBox1.Text.Length, 0); // Move cursor to the end

            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(490, 253);
            Controls.Add(label1);
            Controls.Add(selectDirectoryButton);
            Controls.Add(textBox1);
            Controls.Add(label2);
            Controls.Add(textBox2);
            Controls.Add(downloadButton);
            Name = "github下载器";
            Text = "github下载器";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        // 选择目录
        private void SelectDirectoryButton_Click(object sender, EventArgs e)
        {
            // Show the FolderBrowserDialog and set the selected directory to textBox1
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog.SelectedPath;
            }
        }


        // 点击下载安妮
        private void DownloadButton_Click(object sender, EventArgs e)
        {
            // Get the contents of the textboxes
            string selectedDirectory = textBox1.Text;
            string downloadUrl = textBox2.Text;


            // Check if the URL is valid
            if (IsUrlValid(downloadUrl))
            {
                // URL is valid, continue with your download logic
                // Example: Display a message with the valid URL
                DownloadWithProxy(downloadUrl, selectedDirectory);

                // Add your actual download logic here...
            }
            else
            {
                // URL is not valid, show an error message or take appropriate action
                MessageBox.Show("地址必须是github的: 'https://github.com/'.");
            }
        }

        //判断url合法性
        private bool IsUrlValid(string url)
        {
            // Check if the URL starts with "https://github.com/"
            return url.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase);
        }

        //下载
        private void DownloadWithProxy(string url, string savePath)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    LogToFile("开始下载文件...");

                    // 配置代理设置（如果需要）
                    LogToFile("配置代理设置...");
                    WebProxy proxy = new WebProxy("");
                    proxy.Credentials = new NetworkCredential("", "");
                    webClient.Proxy = proxy;

                    // 获取 GitHub 下载 URL
                    LogToFile("获取 GitHub 下载 URL...");
                    string zipdata = GetGithubDownloadZipUrl(url);
                    JsonDocument jsonDoc = JsonDocument.Parse(zipdata);
                    JsonElement root = jsonDoc.RootElement;
                    string downloadPath = root.GetProperty("props").GetProperty("initialPayload").GetProperty("overview").GetProperty("codeButton").GetProperty("local").GetProperty("platformInfo").GetProperty("zipballUrl").GetString();
                    string downloadUrl = "https://github.com" + downloadPath;

                    // 创建保存文件的目录
                    string directoryPath = Path.GetDirectoryName(savePath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                        LogToFile($"创建目录: {directoryPath}");
                    }

                    // 下载文件
                    LogToFile($"从以下地址下载文件: {downloadUrl}");
                    webClient.DownloadFile(downloadUrl, Path.Combine(savePath, "downloaded_file.zip"));

                    LogToFile("下载成功！");
                    MessageBox.Show($"下载成功");
                }
            }
            catch (WebException ex)
            {
                HandleWebException(ex);
            }
            catch (Exception ex)
            {
                LogToFile($"下载文件时出错: {ex.Message}");
                MessageBox.Show($"下载文件时出错: {ex.Message}");
            }
        }

        private void HandleWebException(WebException ex)
        {
            LogToFile($"WebException: {ex.Message}");

            if (ex.Response is HttpWebResponse response)
            {
                LogToFile($"HTTP Status Code: {response.StatusCode}");

                // Handle specific HTTP status codes if needed
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Handle 404 Not Found error
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Handle 401 Unauthorized error
                }
                // Add more conditions as needed
            }

            // Log the entire exception details if necessary
            LogToFile($"Exception Details: {ex.ToString()}");
        }

        private string GetGithubDownloadZipUrl(string urls)
        {
            try
            {
                LogToFile("获取 GitHub 下载页面内容...");
                string proxyUrl = "";
                string proxyUsername = "";
                string proxyPassword = "";
                string regexPattern = "<script type=\"application/json\" data-target=\"react-partial.embeddedData\">(.*?)</script>";

                // 获取代理请求的 HTML 内容
                string htmlContent = GetContentWithProxy(urls, proxyUrl, proxyUsername, proxyPassword);

                if (!string.IsNullOrEmpty(htmlContent))
                {
                    LogToFile("从 HTML 内容中提取 GitHub 下载 URL...");
                    string extractedContent = ExtractContentWithRegex(htmlContent, regexPattern);

                    if (!string.IsNullOrEmpty(extractedContent))
                    {
                        return extractedContent;
                    }
                }

                LogToFile("未能获取 GitHub 下载 URL。");
                return "";
            }
            catch (Exception ex)
            {
                LogToFile($"获取 GitHub 下载 URL 时出错: {ex.Message}");
                return "";
            }
        }

        private void LogToFile(string logMessage)
        {
            try
            {
                string logPath = "./text.log";
                using (StreamWriter sw = File.AppendText(logPath))
                {
                    sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {logMessage}");
                }
            }
            catch (Exception ex)
            {
                // 处理写入日志文件时的错误
                Console.WriteLine($"写入日志文件时出错: {ex.Message}");
            }
        }

        static string GetContentWithProxy(string url, string proxyUrl, string proxyUsername, string proxyPassword)
        {
            try
            {
                WebProxy proxy = new WebProxy(proxyUrl)
                {
                    Credentials = new NetworkCredential(proxyUsername, proxyPassword)
                };

                HttpClientHandler handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    // 使用 .Result 同步等待获取字符串
                    string htmlContent = httpClient.GetStringAsync(url).Result;
                    return htmlContent;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常: {ex.Message}");
                return null;
            }
        }

        static string ExtractContentWithRegex(string htmlContent, string regexPattern)
        {
            try
            {
                // 使用 Regex.Matches 获取所有匹配项
                MatchCollection matches = Regex.Matches(htmlContent, regexPattern, RegexOptions.Singleline);

                // 检查是否至少有两个匹配项
                if (matches.Count >= 2)
                {
                    // 返回第二个匹配项的内容
                    return matches[1].Groups[1].Value;
                }
                else
                {
                    Console.WriteLine("未找到足够的匹配项");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常: {ex.Message}");
                return null;
            }
        }
    }
}
