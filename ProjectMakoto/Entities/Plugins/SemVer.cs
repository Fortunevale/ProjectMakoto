namespace ProjectMakoto.Plugins;

public class SemVer
{
    public SemVer(int major, int minor, int patch) 
    {
        this.Major = major;
        this.Minor = minor;
        this.Patch = patch;
    }

    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }

    public override string ToString() 
        => $"{Major}.{Minor}.{Patch}";

    public static implicit operator string(SemVer v) 
        => $"{v.Major}.{v.Minor}.{v.Patch}";
    
    public static implicit operator int(SemVer v) 
        => (v.Major * 1000) + (v.Minor * 100) + v.Patch;
}
