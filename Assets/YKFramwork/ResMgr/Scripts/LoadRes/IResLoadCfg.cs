namespace YKFramwork.ResMgr
{
    public interface IResLoadCfg
    {
        bool SimulateAssetBundle { get; }
        string EditorExternalResDir { get; }
        ResJsonData ResData { get; }
        
        string RootABPath { get; }
        
        string RootABUrl { get; }
        
        string EditorResJsonPath { get; }
        
        string AssetBundleVariant { get; }
        
    }
}