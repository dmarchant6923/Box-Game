using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlegroundManager : MonoBehaviour
{
    [HideInInspector] public static int wave = 1;
    [HideInInspector] public static int enemiesKilled = 0;
    bool currentWaveActive = false;
    int wavePoints;
    float timeBetweenWaves = 3;
    float maxHealth = 250;

    Vector2[] spawnLimits = new Vector2[2]; // [0] is transform position, [1] is transform half size

    [HideInInspector] public static bool addToHiScores = false;

    public class Enemy
    {
        public GameObject enemyObject;
        public int enemyPoints;
        
        public Enemy(GameObject enemy, int points)
        {
            enemyObject = enemy;
            enemyPoints = points;
        }
    }

    public class EnemyType
    {
        public Enemy enemylvl1;
        public Enemy enemylvl2;
        public Enemy enemylvl3;
        public EnemyType(Enemy enemy1, Enemy enemy2, Enemy enemy3)
        {
            enemylvl1 = enemy1;
            enemylvl2 = enemy2;
            enemylvl3 = enemy3;
        }

    }

    public GameObject groundedEnemy1;
    public GameObject groundedEnemy2;
    public GameObject groundedEnemy3;

    public GameObject flyingShooter1;
    public GameObject flyingShooter2;
    public GameObject flyingShooter3;

    public GameObject flyingSniper1;
    public GameObject flyingSniper2;
    public GameObject flyingSniper3;

    public GameObject flyingKamikaze1;

    public GameObject flyingShotgun1;
    public GameObject flyingShotgun2;
    public GameObject flyingShotgun3;

    public GameObject groundedVehicle1;
    public GameObject groundedVehicle2;
    public GameObject groundedVehicle3;

    public GameObject mountedTurret1;
    public GameObject mountedTurret2;
    public GameObject mountedTurret3;

    public GameObject wizardShield;
    public GameObject wizardPulse;
    public GameObject wizardAggro;

    public GameObject thunderGuy;

    int groundedEnemy1Points = 1;
    int groundedEnemy2Points = 1;
    int groundedEnemy3Points = 1;

    int flyingShooter1Points = 2;
    int flyingShooter2Points = 4;
    int flyingShooter3Points = 7;

    int flyingSniper1Points = 3;
    int flyingSniper2Points = 5;
    int flyingSniper3Points = 8;

    int flyingKamikaze1Points = 2;

    int flyingShotgun1Points = 3;
    int flyingShotgun2Points = 5;
    int flyingShotgun3Points = 8;

    int groundedVehicle1Points = 10;
    int groundedVehicle2Points = 14;
    int groundedVehicle3Points = 22;

    int mountedTurret1Points = 7;
    int mountedTurret2Points = 12;
    int mountedTurret3Points = 13;

    int wizardShieldPoints = 7;
    int wizardPulsePoints = 7;
    int wizardAggroPoints = 7;

    int thunderGuyPoints = 9;

    Enemy groundedEnemyLvl1;
    Enemy groundedEnemyLvl2;
    Enemy groundedEnemylvl3;

    Enemy flyingShooterLvl1;
    Enemy flyingShooterLvl2;
    Enemy flyingShooterLvl3;

    Enemy flyingSniperLvl1;
    Enemy flyingSniperLvl2;
    Enemy flyingSniperLvl3;

    Enemy flyingKamikazeLvl1;

    Enemy flyingShotgunLvl1;
    Enemy flyingShotgunLvl2;
    Enemy flyingShotgunLvl3;

    Enemy groundedVehicleLvl1;
    Enemy groundedVehicleLvl2;
    Enemy groundedVehicleLvl3;

    Enemy mountedTurretLvl1;
    Enemy mountedTurretLvl2;
    Enemy mountedTurretLvl3;

    Enemy wizardLvl1;
    Enemy wizardLvl2;
    Enemy wizardLvl3;

    Enemy thunderGuyLvl1;

    EnemyType groundedEnemy;
    EnemyType flyingShooter;
    EnemyType flyingSniper;
    EnemyType flyingKamikaze;
    EnemyType flyingShotgun;
    EnemyType groundedVehicle;
    EnemyType mountedTurret;
    EnemyType wizard;
    EnemyType thunder;

    List<EnemyType> enemies = new List<EnemyType>();
    List<GameObject> spawnedEnemies = new List<GameObject>();

    public GameObject heart;
    public GameObject speed;
    public GameObject shield;
    public GameObject heavy;
    public GameObject spikes;
    public GameObject star;

    int obstacleLM;
    int groundLM;
    int boxLM;
    int enemyLM;

    public static bool gameOver = false;
    bool deathActive = false;

    Box boxScript;

    void Start()
    {
        UIManager.stopClock = false;
        UIManager.killToPulse = true;
        gameOver = false;
        deathActive = false;
        boxScript = GameObject.Find("Box").GetComponent<Box>();

        obstacleLM = LayerMask.GetMask("Obstacles");
        groundLM = LayerMask.GetMask("Obstacles", "Platforms");
        boxLM = LayerMask.GetMask("Box");
        enemyLM = LayerMask.GetMask("Enemies");

        Box.dashUnlocked = true;
        Box.teleportUnlocked = true;
        Box.pulseUnlocked = true;

        spawnLimits[0] = transform.position; spawnLimits[1] = transform.lossyScale / 2;

        enemies.Clear();

        groundedEnemyLvl1 = new Enemy(groundedEnemy1, groundedEnemy1Points);
        groundedEnemyLvl2 = new Enemy(groundedEnemy2, groundedEnemy2Points);
        groundedEnemylvl3 = new Enemy(groundedEnemy3, groundedEnemy3Points);

        flyingShooterLvl1 = new Enemy(flyingShooter1, flyingShooter1Points);
        flyingShooterLvl2 = new Enemy(flyingShooter2, flyingShooter2Points);
        flyingShooterLvl3 = new Enemy(flyingShooter3, flyingShooter3Points);

        flyingSniperLvl1 = new Enemy(flyingSniper1, flyingSniper1Points);
        flyingSniperLvl2 = new Enemy(flyingSniper2, flyingSniper2Points);
        flyingSniperLvl3 = new Enemy(flyingSniper3, flyingSniper3Points);

        flyingKamikazeLvl1 = new Enemy(flyingKamikaze1, flyingKamikaze1Points);

        flyingShotgunLvl1 = new Enemy(flyingShotgun1, flyingShotgun1Points);
        flyingShotgunLvl2 = new Enemy(flyingShotgun2, flyingShotgun2Points);
        flyingShotgunLvl3 = new Enemy(flyingShotgun3, flyingShotgun3Points);

        groundedVehicleLvl1 = new Enemy(groundedVehicle1, groundedVehicle1Points);
        groundedVehicleLvl2 = new Enemy(groundedVehicle2, groundedVehicle2Points);
        groundedVehicleLvl3 = new Enemy(groundedVehicle3, groundedVehicle3Points);

        mountedTurretLvl1 = new Enemy(mountedTurret1, mountedTurret1Points);
        mountedTurretLvl2 = new Enemy(mountedTurret2, mountedTurret2Points);
        mountedTurretLvl3 = new Enemy(mountedTurret3, mountedTurret3Points);

        wizardLvl1 = new Enemy(wizardShield, wizardShieldPoints);
        wizardLvl2 = new Enemy(wizardPulse, wizardPulsePoints);
        wizardLvl3 = new Enemy(wizardAggro, wizardAggroPoints);

        thunderGuyLvl1 = new Enemy(thunderGuy, thunderGuyPoints);

        groundedEnemy = new EnemyType(groundedEnemyLvl1, groundedEnemyLvl2, groundedEnemylvl3);
        flyingShooter = new EnemyType(flyingShooterLvl1, flyingShooterLvl2, flyingShooterLvl3);
        flyingSniper = new EnemyType(flyingSniperLvl1, flyingSniperLvl2, flyingSniperLvl3);
        flyingKamikaze = new EnemyType(flyingKamikazeLvl1, flyingKamikazeLvl1, flyingKamikazeLvl1);
        flyingShotgun = new EnemyType(flyingShotgunLvl1, flyingShotgunLvl2, flyingShotgunLvl3);
        groundedVehicle = new EnemyType(groundedVehicleLvl1, groundedVehicleLvl2, groundedVehicleLvl3);
        mountedTurret = new EnemyType(mountedTurretLvl1, mountedTurretLvl2, mountedTurretLvl3);
        wizard = new EnemyType(wizardLvl1, wizardLvl2, wizardLvl3);
        thunder = new EnemyType(thunderGuyLvl1, thunderGuyLvl1, thunderGuyLvl1);

        enemies.Add(groundedEnemy); //0
        enemies.Add(flyingShooter); //1
        enemies.Add(flyingSniper); //2
        enemies.Add(flyingKamikaze); //3
        enemies.Add(flyingShotgun); //4
        enemies.Add(groundedVehicle); //5
        enemies.Add(mountedTurret); //6
        enemies.Add(wizard); //7
        enemies.Add(thunder); //8

        wave = 14;
        enemiesKilled = 0;

        Box.boxHealth = 100;
        UIManager.initialHealth = (int) maxHealth; //250

        addToHiScores = false;
        if ((wave == 0 && Box.boxHealth == 250) || (wave == 14 && Box.boxHealth == 100))
        {
            addToHiScores = true;
        }
        
        StartCoroutine(RoundStart());
    }

    void Update()
    {
        if (currentWaveActive == true)
        {
            for (int i = 0; i < spawnedEnemies.Count; i++)
            {
                if ((spawnedEnemies[i] != null && spawnedEnemies[i].GetComponent<EnemyManager>().enemyWasKilled == true) || spawnedEnemies[i] == null)
                {
                    spawnedEnemies.RemoveAt(i);
                    Debug.Log("Spawned enemies: " + spawnedEnemies.Count);
                    enemiesKilled++;
                    if (UIManager.killToPulse)
                    {
                        UIManager.pulseNoKill = false;
                    }
                }
            }
            if (spawnedEnemies.Count == 0)
            {
                Debug.Log("Wave Finished");
                StartCoroutine(RoundStart());
            }
        }

        if (Box.boxHealth <= 0 && deathActive == false)
        {
            deathActive = true;
            StartCoroutine(Death());
        }
    }

    IEnumerator RoundStart()
    {
        currentWaveActive = false;
        spawnedEnemies.Clear();
        yield return new WaitForSeconds(timeBetweenWaves / 4);
        Box.boxHealth += 10;
        wave++;
        yield return new WaitForSeconds(timeBetweenWaves * 3/4);
        float wavePointMult = 1.5f;
        wavePoints = (int) Mathf.Floor(wave * wavePointMult);
        int wizards = 0;
        int groundedVehicles = 0;

        while (wavePoints > 0)
        {
            int enemyTypeSelected;
            int enemyDifficulty;
            Enemy enemySelected = null;
            // select an enemy type with remaining points
            enemyTypeSelected = Random.Range(0, enemies.Count);
            // determine the enemy difficulty based on wave intervals
            if (wave > 40)
            {
                int[] difficulties = new int[] { 2, 2, 3, 3, 3 };
                enemyDifficulty = difficulties[Random.Range(0, difficulties.Length)];
            }
            else if (wave > 32)
            {
                int[] difficulties = new int[] {2, 2, 2, 3 };
                enemyDifficulty = difficulties[Random.Range(0, difficulties.Length)];
                if (wavePoints > (int)Mathf.Floor(wave * wavePointMult * 0.75f))
                {
                    enemyDifficulty = 3;
                }
            }
            else if (wave > 24)
            {
                int[] difficulties = new int[] { 1, 1, 2, 2, 2, 2, 3 };
                enemyDifficulty = difficulties[Random.Range(0, difficulties.Length)];
            }
            else if (wave > 16)
            {
                int[] difficulties = new int[] { 1, 1, 2, 2, 2 };
                enemyDifficulty = difficulties[Random.Range(0, difficulties.Length)];
                if (wavePoints > (int)Mathf.Floor(wave * 1.8f * 0.75f))
                {
                    enemyDifficulty = 2;
                }
            }
            else if (wave > 8)
            {
                int[] difficulties = new int[] { 1, 1, 2 };
                enemyDifficulty = difficulties[Random.Range(0, difficulties.Length)];
                if (wavePoints > (int)Mathf.Floor(wave * 1.8f * 0.75f))
                {
                    enemyDifficulty = 2;
                }
            }
            else
            {
                enemyDifficulty = 1;
            }

            if (enemyDifficulty == 1)
            {
                enemySelected = enemies[enemyTypeSelected].enemylvl1;
            }
            if (enemyDifficulty == 2)
            {
                enemySelected = enemies[enemyTypeSelected].enemylvl2;
            }
            else if (enemyDifficulty == 3)
            {
                enemySelected = enemies[enemyTypeSelected].enemylvl3;
            }

            // break if the enemy selected costs too many points
            if (enemySelected.enemyPoints > wavePoints)
            {
                continue;
            }
            //only allow a max of 4 grounded enemies to spawn at higher waves
            if (enemies[enemyTypeSelected] == groundedEnemy && wavePoints > enemySelected.enemyPoints * 4)
            {
                continue;
            }
            //only allow a max of 5 kamikaze enemies to spawn at higher waves, and only spawn them at wave 15 or above
            if (enemySelected.enemyObject == flyingKamikaze1 && (wavePoints > enemySelected.enemyPoints * 5 || wave < 15))
            {
                continue;
            }
            //no wizards before wave 17, can't be the first enemy spawned, and can't be more than three. Will also randomly select difficulty irrespective of above
            if (enemyTypeSelected == 7)
            {
                if (wave < 17 || wavePoints == (int)Mathf.Floor(wave * wavePointMult) || wizards > 2)
                {
                    continue;
                }
                else
                {
                    wizards++;
                }
                enemyDifficulty = Random.Range(1, 4);
                if (enemyDifficulty == 1)
                {
                    enemySelected = enemies[enemyTypeSelected].enemylvl1;
                }
                if (enemyDifficulty == 2)
                {
                    enemySelected = enemies[enemyTypeSelected].enemylvl2;
                }
                else if (enemyDifficulty == 3)
                {
                    enemySelected = enemies[enemyTypeSelected].enemylvl3;
                }
            }
            //no grounded vehicles before wave 15 (besides scripted one on wave 10), and max of one
            if (enemies[enemyTypeSelected] == groundedVehicle && wave < 15)
            {
                if (wave < 15 || (wave < 25 && groundedVehicles > 0 && groundedVehicles < 2))
                {
                    continue;
                }
                else
                {
                    groundedVehicles++; 
                }
            }
            //no lvl 2 mounted turrets before wave 15
            if (enemies[enemyTypeSelected] == mountedTurret && enemySelected == enemies[enemyTypeSelected].enemylvl2 && wave < 15)
            {
                continue;
            }
            //no lvl 2 grounded vehicles before wave 20
            if (enemies[enemyTypeSelected] == groundedVehicle && enemySelected == enemies[enemyTypeSelected].enemylvl2 && wave < 20)
            {
                continue;
            }
            //no lvl 3 grounded vehicles before wave 30
            if (enemies[enemyTypeSelected] == groundedVehicle && enemySelected == enemies[enemyTypeSelected].enemylvl3 && wave < 30)
            {
                continue;
            }
            if (enemies[enemyTypeSelected] == thunder && (wave < 20 || wavePoints > 26))
            {
                continue;
            }


            //preset waves
            if (wave == 2)
            {
                enemyTypeSelected = 0; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                if (spawnedEnemies.Count >= 2)
                {
                    wavePoints = 0;
                }
            }
            if (wave == 3)
            {
                enemyTypeSelected = 1; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                if (spawnedEnemies.Count >= 1)
                {
                    wavePoints = 0;
                }
            }
            if (wave == 4)
            {
                enemyTypeSelected = 2; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                if (spawnedEnemies.Count >= 1)
                {
                    wavePoints = 0;
                }
            }
            if (wave == 5)
            {
                enemyTypeSelected = 4; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                if (spawnedEnemies.Count >= 2)
                {
                    wavePoints = 0;
                }
            }
            if (wave == 6)
            {
                enemyTypeSelected = 0; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                if (spawnedEnemies.Count >= 7)
                {
                    wavePoints = 0;
                }
            }
            if (wave == 10)
            {
                enemyTypeSelected = 5; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 0;
            }
            if (wave == 18)
            {
                enemyTypeSelected = 3; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                if (spawnedEnemies.Count >= 6)
                {
                    wavePoints = 0;
                }
            }
            if (wave == 20)
            {
                enemyTypeSelected = 0; enemySelected = enemies[enemyTypeSelected].enemylvl3; wavePoints = 100;
                if (spawnedEnemies.Count >= 3)
                {
                    enemyTypeSelected = 0; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                }
                if (spawnedEnemies.Count >= 5)
                {
                    wavePoints = 0;
                }
            }
            if (wave == 21)
            {
                if (wavePoints == Mathf.Floor(wave * wavePointMult))
                {
                    enemyTypeSelected = 1; enemySelected = enemies[enemyTypeSelected].enemylvl3; wavePoints = 100;
                }
                else
                {
                    enemyTypeSelected = 1; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                }
                if (spawnedEnemies.Count >= 5)
                {
                    wavePoints = 0;
                }
            }
            if (wave == 22)
            {
                if (wavePoints == Mathf.Floor(wave * wavePointMult))
                {
                    enemyTypeSelected = 2; enemySelected = enemies[enemyTypeSelected].enemylvl3; wavePoints = 100;
                }
                else
                {
                    enemyTypeSelected = 2; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                }
                if (spawnedEnemies.Count >= 5)
                {
                    wavePoints = 0;
                }
            }
            if (wave == 23)
            {
                if (wavePoints == Mathf.Floor(wave * wavePointMult))
                {
                    enemyTypeSelected = 4; enemySelected = enemies[enemyTypeSelected].enemylvl3; wavePoints = 100;
                }
                else
                {
                    enemyTypeSelected = 4; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                }
                if (spawnedEnemies.Count >= 5)
                {
                    wavePoints = 0;
                }
            }
            if (wave == 30)
            {
                enemyTypeSelected = 5; enemySelected = enemies[enemyTypeSelected].enemylvl3; wavePoints = 0;
            }



            //determine enemies spawn
            Vector2 spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
            RaycastHit2D spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 0.5f, Vector2.zero, 0f, groundLM);
            RaycastHit2D spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, 5f, Vector2.zero, 0f, boxLM);
            RaycastHit2D spawnGroundCheck = Physics2D.Raycast(spawnCoordinates + Vector2.down, Vector2.down, 3, groundLM);
            if (enemies[enemyTypeSelected] == groundedVehicle)
            {
                spawnGroundCheck = Physics2D.Raycast(spawnCoordinates + Vector2.down, Vector2.down, 3, obstacleLM);
            }
            int numberOfNewCoordinates = 0;
            if (enemies[enemyTypeSelected] == groundedEnemy)
            {
                while (spawnObstacleCheck.collider != null || spawnGroundCheck.collider == null || spawnBoxCheck.collider != null)
                {
                    numberOfNewCoordinates++;
                    spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                               spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
                    spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 0.5f, Vector2.zero, 0f, groundLM);
                    spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, 5f, Vector2.zero, 0f, boxLM);
                    spawnGroundCheck = Physics2D.Raycast(spawnCoordinates + Vector2.down, Vector2.down, 3, groundLM);
                    if (enemies[enemyTypeSelected] == groundedVehicle)
                    {
                        spawnGroundCheck = Physics2D.Raycast(spawnCoordinates + Vector2.down, Vector2.down, 3, obstacleLM);
                    }
                    int rand = Random.Range(0, 5);
                    if (rand == 4)
                    {
                        yield return null;
                    }
                }
            }

            if (enemies[enemyTypeSelected] == groundedVehicle)
            {
                while (spawnObstacleCheck.collider != null || spawnGroundCheck.collider == null || spawnBoxCheck.collider != null ||
                    spawnCoordinates.y > spawnLimits[0].y)// + (spawnLimits[1].y * 2/3))
                {
                    numberOfNewCoordinates++;
                    spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                               spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
                    spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 0.5f, Vector2.zero, 0f, groundLM);
                    spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, 5f, Vector2.zero, 0f, boxLM);
                    spawnGroundCheck = Physics2D.Raycast(spawnCoordinates + Vector2.down, Vector2.down, 3, groundLM);
                    if (enemies[enemyTypeSelected] == groundedVehicle)
                    {
                        spawnGroundCheck = Physics2D.Raycast(spawnCoordinates + Vector2.down, Vector2.down, 3, obstacleLM);
                    }
                    int rand = Random.Range(0, 5);
                    if (rand == 4)
                    {
                        yield return null;
                    }
                }
            }

            RaycastHit2D spawnCircleGroundCheck = Physics2D.CircleCast(spawnCoordinates, 2f, Vector2.zero, 0f, groundLM);
            if (enemies[enemyTypeSelected] == flyingShooter || enemies[enemyTypeSelected] == flyingKamikaze ||
                enemies[enemyTypeSelected] == flyingSniper || enemies[enemyTypeSelected] == flyingShotgun)
            {
                while (spawnCircleGroundCheck.collider != null || spawnBoxCheck.collider != null)
                {
                    spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                        spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
                    spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, 5f, Vector2.zero, 0f, boxLM);
                    spawnCircleGroundCheck = Physics2D.CircleCast(spawnCoordinates, 2f, Vector2.zero, 0f, groundLM);
                    int rand = Random.Range(0, 5);
                    if (rand == 4)
                    {
                        yield return null;
                    }
                }
            }

            RaycastHit2D enemyCircleCheck = Physics2D.CircleCast(spawnCoordinates, 8f, Vector2.zero, 0f, enemyLM);
            RaycastHit2D insideEnemyCirclecheck = Physics2D.CircleCast(spawnCoordinates, 1f, Vector2.zero, 0f, enemyLM);
            if (enemies[enemyTypeSelected] == wizard || enemies[enemyTypeSelected] == thunder)
            {
                while (spawnCircleGroundCheck.collider != null || spawnBoxCheck.collider != null || 
                    enemyCircleCheck.collider == null || insideEnemyCirclecheck.collider != null)
                {
                    spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                        spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
                    spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, 5f, Vector2.zero, 0f, boxLM);
                    spawnCircleGroundCheck = Physics2D.CircleCast(spawnCoordinates, 2f, Vector2.zero, 0f, groundLM);
                    enemyCircleCheck = Physics2D.CircleCast(spawnCoordinates, 8f, Vector2.zero, 0f, enemyLM);
                    insideEnemyCirclecheck = Physics2D.CircleCast(spawnCoordinates, 1f, Vector2.zero, 0f, enemyLM);
                    int rand = Random.Range(0, 5);
                    if (rand == 4)
                    {
                        yield return null;
                    }
                }
            }

            if (enemies[enemyTypeSelected] == mountedTurret)
            {
                RaycastHit2D spawnTurretWallCheck = Physics2D.Raycast(spawnCoordinates + Vector2.left * enemySelected.enemyObject.transform.lossyScale.x * 2,
                    Vector2.right, enemySelected.enemyObject.transform.lossyScale.x * 4, obstacleLM);
                RaycastHit2D spawnTurretCeilingCheck = Physics2D.Raycast(spawnCoordinates,
                    Vector2.up, enemySelected.enemyObject.transform.lossyScale.x * 2, obstacleLM);
                while (spawnObstacleCheck.collider != null || spawnBoxCheck.collider != null ||
                    (spawnTurretWallCheck.collider == null && spawnTurretCeilingCheck.collider == null))
                {
                    spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                        spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
                    spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 0.2f, Vector2.zero, 0f, groundLM);
                    spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, 5f, Vector2.zero, 0f, boxLM);
                    spawnTurretWallCheck = Physics2D.Raycast(spawnCoordinates + Vector2.left * enemySelected.enemyObject.transform.lossyScale.x * 2,
                        Vector2.right, enemySelected.enemyObject.transform.lossyScale.x * 4, obstacleLM);
                    spawnTurretCeilingCheck = Physics2D.Raycast(spawnCoordinates, 
                        Vector2.up, enemySelected.enemyObject.transform.lossyScale.x * 2, obstacleLM);
                    int rand = Random.Range(0, 5);
                    if (rand == 4)
                    {
                        yield return null;
                    }
                }
            }

            //custom spawn coordinates
            if (wave == 10 || wave == 30)
            {
                spawnCoordinates = new Vector2(0, transform.position.y - transform.lossyScale.y * 0.4f);
            }



            GameObject newEnemy;
            newEnemy = Instantiate(enemySelected.enemyObject, spawnCoordinates, Quaternion.identity);
            if (newEnemy.GetComponent<MountedTurret>() != null)
            {
                Debug.Log("you are here");
                SpriteRenderer[] sprites = newEnemy.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer item in sprites)
                {
                    item.enabled = false;
                }
            }
            spawnedEnemies.Add(newEnemy);
            wavePoints -= enemySelected.enemyPoints;
        }
        StartCoroutine(SpawnPerks());
        currentWaveActive = true;
    }
    IEnumerator SpawnPerks()
    {
        GameObject[] list = new GameObject[] {speed, speed, speed, speed, speed, speed,
                                                shield, shield, shield,
                                                heavy, heavy, heavy, heavy,
                                                spikes, spikes, spikes,
                                                star};
        GameObject perk = list[Random.Range(0, list.Length)];
        int rand = Random.Range(0, 3);
        bool spawnPerk;
        if (rand == 2)
        {
            spawnPerk = true;
        }
        else
        {
            spawnPerk = false;
        }
        Debug.Log(rand);

        if (wave % 5 == 0)
        {
            spawnPerk = true;
            perk = heart;
        }
        if (spawnPerk)
        {
            Debug.Log("spawn perk");
            Vector2 spawnCoordinates = new Vector2(spawnLimits[0].x + Random.Range(-1f, 1f) * spawnLimits[1].x,
                spawnLimits[0].y + Random.Range(-1f, 1f) * spawnLimits[1].y);
            RaycastHit2D spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 3f, Vector2.zero, 0f, groundLM);
            RaycastHit2D spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, 5f, Vector2.zero, 0f, boxLM);

            while (spawnObstacleCheck.collider != null || spawnBoxCheck.collider != null)
            {
                spawnCoordinates = new Vector2(spawnLimits[0].x + Random.Range(-1f, 1f) * spawnLimits[1].x,
                    spawnLimits[0].y + Random.Range(-1f, 1f) * spawnLimits[1].y);
                spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 3f, Vector2.zero, 0f, groundLM);
                spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, 5f, Vector2.zero, 0f, boxLM);
            }
            yield return null;

            Instantiate(perk, spawnCoordinates, Quaternion.identity);
        }
    }
    IEnumerator Death()
    {
        UIManager.stopClock = true;
        StartCoroutine(boxScript.DisableInputs(5));
        float timer = 0;
        float window = 0.4f;
        boxScript.GetComponent<Renderer>().sortingLayerName = "Dead Enemy";
        FindObjectOfType<CameraFollowBox>().overridePosition = true;
        FindObjectOfType<CameraFollowBox>().forcedPosition = FindObjectOfType<CameraFollowBox>().transform.position;
        while (timer < window)
        {
            boxScript.GetComponent<BoxCollider2D>().enabled = false;
            boxScript.GetComponent<Rigidbody2D>().gravityScale = 0;
            boxScript.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            boxScript.GetComponent<Rigidbody2D>().angularVelocity = 0;
            BoxVelocity.velocitiesX[0] = 0;
            timer += Time.deltaTime;
            yield return null;
        }
        boxScript.GetComponent<Rigidbody2D>().gravityScale = 6;
        boxScript.GetComponent<Rigidbody2D>().velocity = Vector2.up * 17;
        boxScript.GetComponent<Rigidbody2D>().angularVelocity = 1000;
        boxScript.GetComponent<Rigidbody2D>().angularDrag = 0;
        timer = 0;
        window = 2f;
        while (timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        gameOver = true;
    }
}
