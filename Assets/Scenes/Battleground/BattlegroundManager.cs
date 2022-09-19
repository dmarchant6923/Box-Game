using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlegroundManager : MonoBehaviour
{
    public static int stage = 1;
    public static int wave = 1;
    public static int enemiesKilled = 0;

    public int startingWave = 15;
    public bool usePresetWaves = true;
    public bool infiniteHealth = false;
    public bool invulnerable = false;
    public float startingHealth = 100;
    public bool limitlessPulse = false;
    public bool fastPulse = false;

    public bool debugEnabled = false;

    bool currentWaveActive = false;
    int wavePoints;
    float timeBetweenWaves = 3;
    float maxHealth = 250;
    bool firstWave = true;

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

    public GameObject dupeWizard;

    public GameObject blitz1;
    public GameObject blitz2;
    public GameObject blitz3;

    public GameObject spikeSentry;
    public GameObject starMan;
    public GameObject iglooCannon;

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

    int wizardShieldPoints = 5;
    int wizardPulsePoints = 5;
    int wizardAggroPoints = 5;

    int thunderGuyPoints = 9;

    int dupeWizardPoints = 9;

    int blitz1Points = 3;
    int blitz2Points = 5;
    int blitz3Points = 9;

    int spikeSentryPoints = 9;
    int starManPoints = 9;
    int iglooCannonPoints = 9;

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

    Enemy dupeWizardLvl1;

    Enemy blitzLvl1;
    Enemy blitzLvl2;
    Enemy blitzLvl3;

    Enemy adventureMob1;
    Enemy adventureMob2;
    Enemy adventureMob3;

    EnemyType groundedEnemy;
    EnemyType flyingShooter;
    EnemyType flyingSniper;
    EnemyType flyingKamikaze;
    EnemyType flyingShotgun;
    EnemyType groundedVehicle;
    EnemyType mountedTurret;
    EnemyType wizard;
    EnemyType thunder;
    EnemyType duplicate;
    EnemyType blitz;
    EnemyType adventureMob;

    List<EnemyType> enemies = new List<EnemyType>();
    [HideInInspector] public List<GameObject> spawnedEnemies = new List<GameObject>();

    public GameObject heart;
    public GameObject speed;
    public GameObject shield;
    public GameObject heavy;
    public GameObject spikes;
    public GameObject star;
    public GameObject doubleJump;

    int obstacleLM;
    int groundLM;
    int boxLM;
    int enemyLM;

    public static bool gameOver = false;
    bool deathActive = false;

    Box boxScript;
    public GameObject boxShadow;
    GameObject newShadow;

    void Start()
    {
        UIManager.stopClock = false;
        UIManager.killToPulse = !limitlessPulse;
        gameOver = false;
        deathActive = false;
        firstWave = true;
        boxScript = GameObject.Find("Box").GetComponent<Box>();

        obstacleLM = LayerMask.GetMask("Obstacles");
        groundLM = LayerMask.GetMask("Obstacles", "Platforms");
        boxLM = LayerMask.GetMask("Box");
        enemyLM = LayerMask.GetMask("Enemies");

        Box.dashUnlocked = true;
        Box.teleportUnlocked = true;
        Box.pulseUnlocked = true;
        if (fastPulse)
        {
            GameObject.Find("Box").GetComponent<Box>().pulseCooldown *= 0.25f;
        }

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

        dupeWizardLvl1 = new Enemy(dupeWizard, dupeWizardPoints);

        blitzLvl1 = new Enemy(blitz1, blitz1Points);
        blitzLvl2 = new Enemy(blitz2, blitz2Points);
        blitzLvl3 = new Enemy(blitz3, blitz3Points);

        adventureMob1 = new Enemy(spikeSentry, spikeSentryPoints);
        adventureMob2 = new Enemy(starMan, starManPoints);
        adventureMob3 = new Enemy(iglooCannon, iglooCannonPoints);

        groundedEnemy = new EnemyType(groundedEnemyLvl1, groundedEnemyLvl2, groundedEnemylvl3);
        flyingShooter = new EnemyType(flyingShooterLvl1, flyingShooterLvl2, flyingShooterLvl3);
        flyingSniper = new EnemyType(flyingSniperLvl1, flyingSniperLvl2, flyingSniperLvl3);
        flyingKamikaze = new EnemyType(flyingKamikazeLvl1, flyingKamikazeLvl1, flyingKamikazeLvl1);
        flyingShotgun = new EnemyType(flyingShotgunLvl1, flyingShotgunLvl2, flyingShotgunLvl3);
        groundedVehicle = new EnemyType(groundedVehicleLvl1, groundedVehicleLvl2, groundedVehicleLvl3);
        mountedTurret = new EnemyType(mountedTurretLvl1, mountedTurretLvl2, mountedTurretLvl3);
        wizard = new EnemyType(wizardLvl1, wizardLvl2, wizardLvl3);
        thunder = new EnemyType(thunderGuyLvl1, thunderGuyLvl1, thunderGuyLvl1);
        duplicate = new EnemyType(dupeWizardLvl1, dupeWizardLvl1, dupeWizardLvl1);
        blitz = new EnemyType(blitzLvl1, blitzLvl2, blitzLvl3);
        adventureMob = new EnemyType(adventureMob1, adventureMob2, adventureMob3);

        enemies.Add(groundedEnemy); //0
        enemies.Add(flyingShooter); //1
        enemies.Add(flyingSniper); //2
        enemies.Add(flyingKamikaze); //3
        enemies.Add(flyingShotgun); //4
        enemies.Add(groundedVehicle); //5
        enemies.Add(mountedTurret); //6
        enemies.Add(wizard); //7
        enemies.Add(thunder); //8
        enemies.Add(duplicate); //9
        enemies.Add(blitz); //10
        enemies.Add(adventureMob); //11

        wave = startingWave;
        enemiesKilled = 0;

        Box.boxHealth = startingHealth;
        UIManager.initialHealth = (int) maxHealth; //250

        UIManager.pulseNoKill = false;

        addToHiScores = false;
        if (((wave == 1 && Box.boxHealth == 250) || (wave == 15 && Box.boxHealth == 100)) && infiniteHealth == false && invulnerable == false && usePresetWaves)
        {
            addToHiScores = true;
        }

        if (debugEnabled)
        {
            GetComponent<SpriteRenderer>().enabled = true;
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
                    enemiesKilled++;
                    if (UIManager.killToPulse)
                    {
                        UIManager.pulseNoKill = false;
                    }
                }
            }
            if (spawnedEnemies.Count == 0 && FindObjectOfType<StarManProjectile>() == null)
            {
                StartCoroutine(RoundStart());
            }
        }

        if (Box.boxHealth <= 0 && deathActive == false)
        {
            deathActive = true;
            StartCoroutine(Death());
        }




        if (infiniteHealth == Box.boxHealth < maxHealth)
        {
            Box.boxHealth = maxHealth;
        }
        if (invulnerable)
        {
            Box.isInvulnerable = true;
        }
    }

    IEnumerator RoundStart()
    {
        currentWaveActive = false;
        spawnedEnemies.Clear();
        yield return new WaitForSeconds(timeBetweenWaves / 4);
        if (firstWave == false && deathActive == false)
        {
            Box.boxHealth += 10;
            UIManager.pulseNoKill = false;
            wave++;
        }
        yield return new WaitForSeconds(timeBetweenWaves * 3/4);
        float wavePointMult = 1.5f;
        if (wave >= 40)
        {
            wavePointMult += (wave - 39) * 1.5f / 30;
        }
        int maxPoints = (int)Mathf.Floor(wave * wavePointMult);
        wavePoints = maxPoints;
        if (debugEnabled)
        {
            Debug.Log("wave points: " + wavePoints);
        }
        int wizards = 0;
        int dupeWizards = 0;
        int groundedVehicles = 0;
        int thunders = 0;

        while (wavePoints > 0)
        {
            int coordinateIterations = 1;

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
                if (wavePoints > (int) maxPoints * 0.75f)
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
            //only allow a max of 5 kamikaze enemies to spawn at higher waves, and only spawn them at wave 18 or above
            if (enemySelected.enemyObject == flyingKamikaze1 && (wavePoints > enemySelected.enemyPoints * 5 || wave < 18))
            {
                continue;
            }
            //no wizards before wave 18, can't be the first enemy spawned, and can't be more than three. Will also randomly select difficulty irrespective of above
            if (enemyTypeSelected == 7)
            {
                if (wave < 18 || wavePoints == maxPoints || wizards > 2)
                {
                    continue;
                }
                wizards++;
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
                if (wave < 15 || (wave < 35 && groundedVehicles > 0) || (wave >= 35 && groundedVehicles > 2))
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
            //no thunders before wave 20 and no more than 2 thunders. No more than 1 thunder before wave 30
            if (enemies[enemyTypeSelected] == thunder)
            {
                if (wave < 20 || thunders > 1 || wavePoints == maxPoints)
                {
                    continue;
                }
                if (wave < 30 && thunders > 0)
                {
                    continue;
                }
                else
                {
                    thunders++;
                }
            }
            //no duplicate wizards before wave 25
            if (enemies[enemyTypeSelected] == duplicate)
            { 
                if (wave < 25 || dupeWizards >= 1 || wavePoints == maxPoints)
                {
                    continue;
                }
                else
                {
                    dupeWizards++;
                }
            }
            //no adventure mobs before wave 20
            if (enemies[enemyTypeSelected] == adventureMob)
            {
                if (wave < 20)
                {
                    continue;
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


            //preset waves
            if (usePresetWaves)
            {
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
                    if (spawnedEnemies.Count >= 6)
                    {
                        wavePoints = 0;
                    }
                }
                if (wave == 10)
                {
                    enemyTypeSelected = 5; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 0;
                }
                if (wave == 15 && wavePoints != maxPoints && wizards == 0)
                {
                    enemyTypeSelected = 7;
                    enemySelected = enemies[enemyTypeSelected].enemylvl1;
                    wizards++;
                }
                if (wave == 16 && wavePoints != maxPoints && wizards == 0)
                {
                    enemyTypeSelected = 7;
                    enemySelected = enemies[enemyTypeSelected].enemylvl2;
                    wizards++;
                }
                if (wave == 17 && wavePoints != maxPoints && wizards == 0)
                {
                    enemyTypeSelected = 7;
                    enemySelected = enemies[enemyTypeSelected].enemylvl3;
                    wizards++;
                }
                if (wave == 19)
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
                    if (wavePoints == maxPoints)
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
                    if (wavePoints == maxPoints)
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
                    if (wavePoints == maxPoints)
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
                if (wave == 24)
                {
                    if (wavePoints == maxPoints)
                    {
                        enemyTypeSelected = 10; enemySelected = enemies[enemyTypeSelected].enemylvl3; wavePoints = 100;
                    }
                    else
                    {
                        enemyTypeSelected = 10; enemySelected = enemies[enemyTypeSelected].enemylvl1; wavePoints = 100;
                    }
                    if (spawnedEnemies.Count >= 4)
                    {
                        wavePoints = 0;
                    }
                }
                if (wave == 30)
                {
                    enemyTypeSelected = 5; enemySelected = enemies[enemyTypeSelected].enemylvl3; wavePoints = 0;
                }
                if (wave == 35)
                {
                    enemyTypeSelected = 11; enemySelected = enemies[enemyTypeSelected].enemylvl2; wavePoints = 100;
                    if (spawnedEnemies.Count >= 3)
                    {
                        wavePoints = 0;
                    }
                }
            }


            //raycast and circlecast variables
            float spawnBoxRadius = 5f;
            float insideEnemyRadius = 0.5f;
            int returnNullNum = 30;

            //determine initial spawn coordinates
            Vector2 spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
            //check if there is ground within a small radius (ground being obstacles or platforms), to prevent spawning inside the ground
            RaycastHit2D spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 0.5f, Vector2.zero, 0f, groundLM);
            //check if the player is within a certain radius
            RaycastHit2D spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, spawnBoxRadius, Vector2.zero, 0f, boxLM);
            //check if there is ground in a straight line down (ground being obstacles or platforms)
            RaycastHit2D spawnGroundCheck = Physics2D.Raycast(spawnCoordinates + Vector2.down, Vector2.down, 3, groundLM);
            //check if there is an enemy within a small radius, to prevent enemies from spawning inside each other
            RaycastHit2D insideEnemyCheck = Physics2D.CircleCast(spawnCoordinates, insideEnemyRadius, Vector2.zero, 0f, enemyLM);


            //grounded enemies
            if (enemies[enemyTypeSelected] == groundedEnemy)
            {
                //coordinate successful when not inside the ground, the box is not nearby, there is ground below, and not inside an enemy
                while (spawnObstacleCheck.collider != null || spawnGroundCheck.collider == null || 
                       spawnBoxCheck.collider != null || insideEnemyCheck.collider != null)
                {
                    DrawSpawnCoordinates(spawnCoordinates);

                    spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                               spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
                    spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 0.5f, Vector2.zero, 0f, groundLM);
                    spawnGroundCheck = Physics2D.Raycast(spawnCoordinates + Vector2.down, Vector2.down, 3, groundLM);
                    spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, spawnBoxRadius, Vector2.zero, 0f, boxLM);
                    insideEnemyCheck = Physics2D.CircleCast(spawnCoordinates, insideEnemyRadius, Vector2.zero, 0f, enemyLM);

                    if (coordinateIterations % returnNullNum == 0 && coordinateIterations > 0)
                    {
                        yield return null;
                    }

                    coordinateIterations++;
                }
            }

            //grounded vehicles
            if (enemies[enemyTypeSelected] == groundedVehicle)
            {
                //change downwards ground check to only see obstacles and ignore platforms
                spawnGroundCheck = Physics2D.Raycast(spawnCoordinates + Vector2.down, Vector2.down, 3, obstacleLM);

                //coordinate successful when not inside the ground, the box is not nearby, there is an obstacle below,
                //y coordinate is in the lower half of available range, and not inside an enemy
                while (spawnObstacleCheck.collider != null || spawnGroundCheck.collider == null || spawnBoxCheck.collider != null ||
                    insideEnemyCheck.collider != null || spawnCoordinates.y > spawnLimits[0].y)// + (spawnLimits[1].y * 2/3))
                {
                    DrawSpawnCoordinates(spawnCoordinates);

                    spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                               spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
                    spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 0.5f, Vector2.zero, 0f, groundLM);
                    spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, spawnBoxRadius, Vector2.zero, 0f, boxLM);
                    spawnGroundCheck = Physics2D.Raycast(spawnCoordinates + Vector2.down, Vector2.down, 3, obstacleLM);
                    insideEnemyCheck = Physics2D.CircleCast(spawnCoordinates, insideEnemyRadius, Vector2.zero, 0f, enemyLM);

                    if (coordinateIterations % returnNullNum == 0 && coordinateIterations > 0)
                    {
                        yield return null;
                    }

                    coordinateIterations++;
                }
            }

            //flying enemies, blitz, sentry and star man
            //check if there is ground within a radius (ground being obstacles or platforms) to make sure enemy spawns a distance away from ground
            RaycastHit2D spawnCircleGroundCheck = Physics2D.CircleCast(spawnCoordinates, 2f, Vector2.zero, 0f, groundLM);
            if (enemies[enemyTypeSelected] == flyingShooter || enemies[enemyTypeSelected] == flyingKamikaze ||
                enemies[enemyTypeSelected] == flyingSniper || enemies[enemyTypeSelected] == flyingShotgun ||
                enemies[enemyTypeSelected] == blitz || (enemies[enemyTypeSelected] == adventureMob && (enemyDifficulty == 1 || enemyDifficulty == 2)))
            {
                //coordinate successful when ground is not nearby, the box is not nearby, and not inside an enemy
                while (spawnCircleGroundCheck.collider != null || spawnBoxCheck.collider != null || insideEnemyCheck.collider != null)
                {
                    DrawSpawnCoordinates(spawnCoordinates);

                    spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                        spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
                    spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, spawnBoxRadius, Vector2.zero, 0f, boxLM);
                    spawnCircleGroundCheck = Physics2D.CircleCast(spawnCoordinates, 2f, Vector2.zero, 0f, groundLM);
                    insideEnemyCheck = Physics2D.CircleCast(spawnCoordinates, insideEnemyRadius, Vector2.zero, 0f, enemyLM);

                    if (coordinateIterations % returnNullNum == 0 && coordinateIterations > 0)
                    {
                        yield return null;
                    }

                    coordinateIterations++;
                }
            }

            //wizard, thunder and dupe wizard
            //check if there is an enemy within a radius
            RaycastHit2D enemyCircleCheck = Physics2D.CircleCast(spawnCoordinates, 8f, Vector2.zero, 0f, enemyLM);
            if (enemies[enemyTypeSelected] == wizard || enemies[enemyTypeSelected] == thunder || enemies[enemyTypeSelected] == duplicate)
            {
                //coordinate successful when ground is not nearby, the box is not nearby, and when there is an enemy nearby but not too close
                while (spawnCircleGroundCheck.collider != null || spawnBoxCheck.collider != null || 
                    enemyCircleCheck.collider == null || insideEnemyCheck.collider != null)
                {
                    DrawSpawnCoordinates(spawnCoordinates);

                    spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                        spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
                    spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, spawnBoxRadius, Vector2.zero, 0f, boxLM);
                    spawnCircleGroundCheck = Physics2D.CircleCast(spawnCoordinates, 2f, Vector2.zero, 0f, groundLM);
                    enemyCircleCheck = Physics2D.CircleCast(spawnCoordinates, 8f, Vector2.zero, 0f, enemyLM);
                    insideEnemyCheck = Physics2D.CircleCast(spawnCoordinates, insideEnemyRadius, Vector2.zero, 0f, enemyLM);

                    if (coordinateIterations % returnNullNum == 0 && coordinateIterations > 0)
                    {
                        yield return null;
                    }

                    coordinateIterations++;
                }
            }

            //mounted turret and igloo cannon
            if (enemies[enemyTypeSelected] == mountedTurret || (enemies[enemyTypeSelected] == adventureMob && enemyDifficulty == 3))
            {
                //check if there is a wall to the left or right
                RaycastHit2D spawnTurretWallCheck = Physics2D.Raycast(spawnCoordinates + Vector2.left * enemySelected.enemyObject.transform.lossyScale.x * 2,
                    Vector2.right, enemySelected.enemyObject.transform.lossyScale.x * 4, obstacleLM);
                //check if there is a ceiling above
                RaycastHit2D spawnTurretCeilingCheck = Physics2D.Raycast(spawnCoordinates,
                    Vector2.up, enemySelected.enemyObject.transform.lossyScale.x * 2, obstacleLM);
                bool recheck = false;
                
                //coordinate successful when not inside the ground, the box is not nearby, there is a wall or ceiling nearby,
                //the wall/ceiling is not a hazard, and not inside an enemy
                while (spawnObstacleCheck.collider != null || spawnBoxCheck.collider != null || insideEnemyCheck.collider != null ||
                    (spawnTurretWallCheck.collider == null && spawnTurretCeilingCheck.collider == null) || recheck)
                {
                    DrawSpawnCoordinates(spawnCoordinates);

                    recheck = false;
                    spawnCoordinates = new Vector2(spawnLimits[0].x + (Random.Range(-1f, 1f) * spawnLimits[1].x),
                        spawnLimits[0].y + (Random.Range(-1f, 1f) * spawnLimits[1].y));
                    spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 0.2f, Vector2.zero, 0f, groundLM);
                    spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, spawnBoxRadius, Vector2.zero, 0f, boxLM);
                    spawnTurretWallCheck = Physics2D.Raycast(spawnCoordinates + Vector2.left * enemySelected.enemyObject.transform.lossyScale.x * 2,
                        Vector2.right, enemySelected.enemyObject.transform.lossyScale.x * 4, obstacleLM);
                    spawnTurretCeilingCheck = Physics2D.Raycast(spawnCoordinates, 
                        Vector2.up, enemySelected.enemyObject.transform.lossyScale.x * 2, obstacleLM);
                    insideEnemyCheck = Physics2D.CircleCast(spawnCoordinates, insideEnemyRadius, Vector2.zero, 0f, enemyLM);
                    if ((spawnTurretWallCheck.collider != null && spawnTurretWallCheck.collider.GetComponent<Hazards>() != null) ||
                        (spawnTurretCeilingCheck.collider != null && spawnTurretCeilingCheck.collider.GetComponent<Hazards>() != null))
                    {
                        recheck = true;
                    }

                    if (coordinateIterations % returnNullNum == 0 && coordinateIterations > 0)
                    {
                        yield return null;
                    }

                    coordinateIterations++;
                }

                RaycastHit2D rayUp = Physics2D.Raycast(spawnCoordinates, Vector2.up, enemySelected.enemyObject.transform.lossyScale.x * 2, obstacleLM);
                RaycastHit2D rayLeft = Physics2D.Raycast(spawnCoordinates, Vector2.left, enemySelected.enemyObject.transform.lossyScale.x * 2, obstacleLM);
                RaycastHit2D rayRight = Physics2D.Raycast(spawnCoordinates, Vector2.right, enemySelected.enemyObject.transform.lossyScale.x * 2, obstacleLM);

                float offsetFromWallMult = 0.15f;
                if (rayLeft.collider != null)
                {
                    spawnCoordinates = rayLeft.point + Vector2.right * enemySelected.enemyObject.transform.lossyScale.x * offsetFromWallMult;
                }
                else if (rayRight.collider != null)
                {
                    spawnCoordinates = rayRight.point + Vector2.left * enemySelected.enemyObject.transform.lossyScale.x * offsetFromWallMult;
                }
                else if (rayUp.collider != null)
                {
                    spawnCoordinates = rayUp.point + Vector2.down * enemySelected.enemyObject.transform.lossyScale.x * offsetFromWallMult;
                }
            }

            //custom spawn coordinates
            if (wave == 10 || wave == 30)
            {
                spawnCoordinates = new Vector2(0, transform.position.y - transform.lossyScale.y * 0.4f);
            }

            GameObject newEnemy;
            newEnemy = Instantiate(enemySelected.enemyObject, spawnCoordinates, Quaternion.identity);
            if (newEnemy.GetComponent<EnemyBehavior_Turret>() != null)
            {
                newEnemy.GetComponent<EnemyBehavior_Turret>().canAttachToGround = false;
            }
            spawnedEnemies.Add(newEnemy);
            wavePoints -= enemySelected.enemyPoints;
            if (debugEnabled)
            {
                Debug.Log("iterations: " + coordinateIterations + ". enemy spawned: " + enemySelected.enemyObject + ". points left: " + wavePoints);
                Debug.DrawRay(spawnCoordinates + Vector2.left * 2, Vector2.right * 4, Color.green);
                Debug.DrawRay(spawnCoordinates + Vector2.down * 2, Vector2.up * 4, Color.green);
                Debug.DrawRay(spawnCoordinates + Vector2.one.normalized * 2, -Vector2.one.normalized * 4, Color.green);
                Debug.DrawRay(spawnCoordinates + new Vector2(1, -1).normalized * 2, new Vector2(-1, 1).normalized * 4, Color.green);
            }
            yield return new WaitForFixedUpdate();
        }
        SpawnPerks();
        firstWave = false;
        currentWaveActive = true;
        if (debugEnabled)
        {
            Debug.Log("finished spawning");
        }
    }
    void SpawnPerks()
    {
        GameObject[] list = new GameObject[] {speed, speed, speed, speed,
                                                shield, shield, shield,
                                                heavy, heavy,
                                                spikes, spikes,
                                                doubleJump, doubleJump,
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

        if (wave % 5 == 0 && firstWave == false)
        {
            spawnPerk = true;
            perk = heart;
        }
        if (spawnPerk)
        {
            Vector2 spawnCoordinates = new Vector2(spawnLimits[0].x + Random.Range(-1f, 1f) * spawnLimits[1].x,
                spawnLimits[0].y + Random.Range(-1f, 1f) * spawnLimits[1].y);
            RaycastHit2D spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 2f, Vector2.zero, 0f, groundLM);
            RaycastHit2D spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, 5f, Vector2.zero, 0f, boxLM);

            while (spawnObstacleCheck.collider != null || spawnBoxCheck.collider != null)
            {
                spawnCoordinates = new Vector2(spawnLimits[0].x + Random.Range(-1f, 1f) * spawnLimits[1].x,
                    spawnLimits[0].y + Random.Range(-1f, 1f) * spawnLimits[1].y);
                spawnObstacleCheck = Physics2D.CircleCast(spawnCoordinates, 2f, Vector2.zero, 0f, groundLM);
                spawnBoxCheck = Physics2D.CircleCast(spawnCoordinates, 5f, Vector2.zero, 0f, boxLM);
            }

            //rand = Random.Range(0, perkSpawns.transform.childCount);
            //Vector2 spawnCoordinates = perkSpawns.transform.GetChild(rand).transform.position;

            Instantiate(perk, spawnCoordinates, Quaternion.identity);
        }
    }
    void DrawSpawnCoordinates(Vector2 coordinates)
    {
        if (debugEnabled)
        {
            Debug.DrawRay(coordinates + Vector2.left * 0.5f, Vector2.right, Color.red);
            Debug.DrawRay(coordinates + Vector2.down * 0.5f, Vector2.up, Color.red);
            Debug.DrawRay(coordinates + Vector2.one * 0.5f, -Vector2.one, Color.red);
            Debug.DrawRay(coordinates + new Vector2(1,-1) * 0.5f, new Vector2(-1,1), Color.red);
        }
    }
    IEnumerator Death()
    {
        BoxPerks.buffActive = false;
        UIManager.stopClock = true;
        StartCoroutine(boxScript.DisableInputs(20));
        float timer = 0;
        float window = 0.4f;
        boxScript.GetComponent<Renderer>().sortingLayerName = "Dead Enemy";
        FindObjectOfType<CameraFollowBox>().overridePosition = true;
        FindObjectOfType<CameraFollowBox>().forcedPosition = FindObjectOfType<CameraFollowBox>().transform.position;
        while (Box.boxHitstopActive)
        {
            yield return null;
        }
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

        newShadow = Instantiate(boxShadow, boxScript.transform.position, Quaternion.identity);
        timer = 0;
        window = 2f;
        while (timer < window)
        {
            newShadow.transform.position = new Vector2(boxScript.transform.position.x, boxScript.transform.position.y) + new Vector2(0.25f, -0.25f);
            newShadow.GetComponent<Rigidbody2D>().rotation = boxScript.GetComponent<Rigidbody2D>().rotation;
            timer += Time.deltaTime;
            yield return null;
        }
        gameOver = true;
    }
}
