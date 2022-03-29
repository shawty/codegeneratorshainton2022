using System;

namespace MyCompany.Application;

partial class Program
{
  static void Main()
  {
    Console.WriteLine("File Source Generator Test");

    var jsonModel = new JsonModel();

    jsonModel.LoadXml("TestXml.xml");
    jsonModel.SaveJson("TestXml.json");

  }

}