using System.Collections.Generic;

public class ProjectileHolder<T> where T : ProjectileBase<T>
{
    private ProjectileData _projectileData; 
    private List<T> _projectileDeActiveList;
    private List<T> _projectileActiveList;

    public ProjectileHolder()
    {
        _projectileActiveList = new List<T>();
        _projectileDeActiveList = new List<T>();
    }
    
    
}
