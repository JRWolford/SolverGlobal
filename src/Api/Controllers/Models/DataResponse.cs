namespace Api.Controllers.Models
{
  public class DataResponse
  {
    public string DatabaseName { get; set; }
    public string TableName { get; set; }
    public string ElapsedTime { get; set; }
    public long NumberOfRecords { get; set; }
    public object Data { get; set; }
  }
}