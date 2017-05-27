namespace ODataSample.Web.Models
{
	public class Permissions
	{
		public bool Create { get; set; }
		public bool Read { get; set; }
		public bool Update { get; set; }
		public bool Delete { get; set; }

		public Permissions() { }

		public Permissions(bool create, bool read, bool update, bool delete)
		{
			Create = create;
			Read = read;
			Update = update;
			Delete = delete;
		}
	}
}