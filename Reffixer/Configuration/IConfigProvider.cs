namespace Reffixer.Configuration
{
	internal interface IConfigProvider
	{
		T Load<T>(string filePath) where T : class;
	}
}