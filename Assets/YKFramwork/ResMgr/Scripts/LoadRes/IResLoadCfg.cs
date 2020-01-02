namespace YKFramework.ResMgr
{
    public interface IResLoadCfg
    {
        bool SimulateAssetBundle { get; }
        
        ResJsonData ResData { get; }
        
        string RootABPath { get; }
        
        string RootABUrl { get; }
        
        string AssetBundleVariant { get; }
        
    }
}