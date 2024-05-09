namespace TMS_Project.Models
{
    public interface ISoftDeleteTable
    {
        DateTime? UpdatedAt {  get; set; }
        int? UpdatedBy { get; set; }
        bool? MDelete { get; set; }
    }
}
