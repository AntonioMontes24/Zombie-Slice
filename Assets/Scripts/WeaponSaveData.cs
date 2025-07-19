[System.Serializable]
public class WeaponSaveData
{
    public string weaponId;
    public int currentAmmo;
    public int reserveAmmo;

    public WeaponSaveData() { }

    public WeaponSaveData(string weaponId, int currentAmmo, int reserveAmmo)
    {
        this.weaponId = weaponId;
        this.currentAmmo = currentAmmo;
        this.reserveAmmo = reserveAmmo;
    }
}