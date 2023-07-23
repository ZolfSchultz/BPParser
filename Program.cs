using System.Data;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Configuration;


var path = System.Configuration.ConfigurationManager.AppSettings["Path"];
//var path = @"C:\Users\user\Desktop\";
path = path + "BPCataloge.xml";
Console.WriteLine("Parsing BP site");

var dataSet = new DataSet("Main");
var dataTable = new DataTable("Data");
dataSet.Tables.Add(dataTable);
var idColumn = new DataColumn("Id", Type.GetType("System.Int32"));
idColumn.Unique = true; // столбец будет иметь уникальное значение
idColumn.AllowDBNull = false; // не может принимать null
idColumn.AutoIncrement = true; // будет автоинкрементироваться
idColumn.AutoIncrementSeed = 1; // начальное значение
idColumn.AutoIncrementStep = 1; // приращении при добавлении новой строки

var catalogueNo = new DataColumn("Catalogue No", Type.GetType("System.String"));
var packSize = new DataColumn("Pack Size", Type.GetType("System.String"));
var substanceName = new DataColumn("Substance Name", Type.GetType("System.String"));
var declaredContent = new DataColumn("Declared Content", Type.GetType("System.String"));
var currentBatch = new DataColumn("Current Batch", Type.GetType("System.String"));
dataTable.Columns.Add(idColumn);
dataTable.Columns.Add(catalogueNo);
dataTable.Columns.Add(packSize);
dataTable.Columns.Add(substanceName);
dataTable.Columns.Add(declaredContent);
dataTable.Columns.Add(currentBatch);
dataTable.PrimaryKey = new[] { dataTable.Columns["Id"] };


string cataloge = null;
string mass = null;
string name = null;
string additioninfo = null;
string batch = null;


//string[] htmlstring = new string[10];

for (var i = 0; i < 10; i++)
{
    var j = i + 1;
    var url = "https://www.pharmacopoeia.com/Catalogue/Products?page=" + j + "&page-size=100&text=&starts-with=";

    using (var client = new HttpClient())
    {
        using (var response = client.GetAsync(url).Result)
        {
            using (var content = response.Content)
            {
                var result = content.ReadAsStringAsync().Result;
                //Console.WriteLine(result);
                //htmlstring[i] = result;
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(result);
                var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"main\"]/div/div/div/div[3]/table/tbody/tr");
                //var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"main\"]/div/div/div/div[3]/table/tbody");
                foreach (var node in htmlNodes)
                {
                    var htmlstringing = node.OuterHtml;
                    var htmlDoc2 = new HtmlDocument();
                    htmlDoc2.LoadHtml(htmlstringing);
                    var htmlBody = htmlDoc2.DocumentNode.SelectNodes("//td");
                    for (var index = 0; index < htmlBody.Count; index++)
                    {
                        var node2 = htmlBody[index];
                        var htmlstringing2 = node2.OuterHtml;

                        if (index == 0)
                        {
                            var word = htmlstringing2.Split("</div>");
                            var word2 = word[0].Split("<div>");
                            var word3 = word2[1].Split("\r\n");
                            var words = word3[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            cataloge = words[0];
                            //Console.WriteLine(cataloge);
                        }

                        if (index == 1)
                        {
                            var word = htmlstringing2.Split("</td>");
                            var word2 = word[0].Split("<td>");
                            mass = word2[1];
                            //Console.WriteLine(mass);
                        }

                        if (index == 2)
                        {
                            var word = htmlstringing2.Split('>');
                            var word2 = word[2].Split("<");
                            name = word2[0];
                            //Console.WriteLine(name);
                        }

                        if (index == 3)
                        {
                            var word = htmlstringing2.Split("</td>");
                            var word2 = word[0].Split("<td>");
                            var word3 = word2[1].Split("\r\n");
                            additioninfo = word3[1];
                            var pattern = @"(<sub>|</sub>| <br>)";
                            additioninfo = Regex.Replace(additioninfo, pattern, string.Empty);
                            //Console.WriteLine(additioninfo);
                        }

                        //Console.WriteLine(node2.OuterHtml);
                        if (index == 4)
                        {
                            var word = htmlstringing2.Split("</td>");
                            var word2 = word[0].Split("<td>");
                            batch = word2[1];
                            //Console.WriteLine(batch);
                        }

                        //Thread.Sleep(2000);
                        if (index == 5) continue;
                    }

                    var myNewRow = dataTable.NewRow();
                    //myNewRow["Id"] = 1;
                    myNewRow["Catalogue No"] = cataloge;
                    myNewRow["Pack Size"] = mass;
                    myNewRow["Substance Name"] = name;
                    myNewRow["Declared Content"] = additioninfo;
                    myNewRow["Current Batch"] = batch;
                    dataTable.Rows.Add(myNewRow);
                }
                //Console.WriteLine(htmlBody.OuterHtml);
            }
        }
    }
}

var dsXml = dataSet.GetXml();
using (var fs = new StreamWriter(path)) // XML File Path
{
    dataSet.WriteXml(fs);
}