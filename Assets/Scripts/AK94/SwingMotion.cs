using UnityEngine;

public class SwingMotion : MonoBehaviour
{
    [SerializeField]
    private int anchorLayer;

    //Toujours pratique
    private Camera mainCamera;
    private RotationHandler rotationHandler;
    private float offsetScalar = .002f;

    //Pour bien placer le viseur
    private Vector3 aimPosition;

    //Tous nos sprites
    private Transform[] spriteChildren;
    private float scaleScalar = 0.05f;
    private float pixelSize;

    //Pour nos controles
    private bool leftClick, rightClick;
    private Vector2 mouseDelta;
    private float currentXMovement, currentYMovement;

    //Pour les mouvements de notre arme
    private float currentXPosition, currentYPosition;
    private float swayPosition, swayLimit = Mathf.PI / 2f, swaySpeed = Mathf.PI,
        swayXScalar = .1f, swayYScalar = .03f;
    private int swayDirection = 1;

    private void Awake()
    {
        InitializeVariables();
        SetTheAimPoint();
        InitializeScales();
    }

    private void LateUpdate()
    {
        GatherInputs();
        WeaponSway();
        FakeDepth();
    }

    private void InitializeVariables()
    {
        mainCamera = Camera.main;
        rotationHandler = GetComponentInParent<RotationHandler>();
    }

    private void SetTheAimPoint()
    {
        //Le pivot de notre arme est le centre de notre viseur
        //Donc notre viseur peut etre utilise pour notre crosshair
        //On veut placer ce crosshair sur la ligne du tiers inferieur
        //Comme ca on inciterait un peu le joueur a regarder vers le haut en permanence
        aimPosition = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f,
            Screen.height / 3f, mainCamera.nearClipPlane + offsetScalar));
        transform.localPosition = aimPosition;
    }

    private void InitializeScales()
    {
        //On recupere les enfants, en leur donnant la bonne taille et position
        spriteChildren = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            spriteChildren[i] = transform.GetChild(i);
            spriteChildren[i].localScale = new Vector3(1f - i * scaleScalar,
                1f - i * scaleScalar, 1f);
            spriteChildren[i].localPosition = Vector3.forward * offsetScalar * i;
            spriteChildren[i].GetComponent<SpriteRenderer>().sortingOrder = -i;
        }

        //La taille d'un de nos pixels dans le monde de la scene
        pixelSize = .5f /
            spriteChildren[0].GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
    }

    private void GatherInputs()
    {
        //On recupere ce que le joueur vient de faire
        leftClick = rotationHandler.leftClick;
        rightClick = rotationHandler.rightClick;
        mouseDelta = rotationHandler.GetMouseDelta();

        //On calcule les angles de la souris
        currentXMovement = Quaternion.Euler(mouseDelta.x, 0f, 0f).x / Time.deltaTime;
        currentYMovement = Quaternion.Euler(0f, mouseDelta.y, 0f).y / Time.deltaTime;
    }

    private void WeaponSway()
    {
        //Fonction pour le "mouvement" au sol

        if (rightClick)
        {
            //Si on est a la limite, on snap et on part dans l'autre sens
            if (Mathf.Abs(swayPosition) > swayLimit)
            {
                swayPosition = swayDirection * swayLimit;
                swayDirection = -swayDirection;
            }
            //Sinon, on avance tranquillement
            else swayPosition += swayDirection * swaySpeed * Time.deltaTime;
        }

        //Plus de mouvement donc retour au centre
        else
        {
            //Si on est assez proche, on snap
            //Sinon, on revient au centre
            swayPosition = Mathf.Abs(swayPosition) < swaySpeed * 2f * Time.deltaTime ?
                0 : swayPosition - Mathf.Sign(swayPosition) * swaySpeed *
                2f * Time.deltaTime;
        }

        //On applique notre position
        transform.localPosition = aimPosition + new Vector3(Mathf.Sin(swayPosition)
            * swayXScalar, (Mathf.Cos(swayPosition) - 1) * swayYScalar, 0f);
    }

    private void FakeDepth()
    {
        //Fonction pour le mouvement de camera

        //Bouger la camera si on est en mouvement
        if (leftClick)
        {
            currentXPosition -= currentXMovement / 30f;
            currentYPosition += currentYMovement / 15f;
        }

        //Un peu de retour a zero
        currentXPosition = Mathf.Abs(currentXPosition) > 3 ?
            Mathf.Sign(currentXPosition) * 3 :
            currentXPosition - Time.deltaTime * Mathf.Sign(currentXPosition) * 5f;

        currentYPosition = Mathf.Abs(currentYPosition) > 1.5f ?
            Mathf.Sign(currentYPosition) * 1.5f :
            currentYPosition - Time.deltaTime * Mathf.Sign(currentYPosition) * 5f;

        //On applique cette position aux sprites
        for (int i = 0; i < spriteChildren.Length; i++)
        {
            spriteChildren[i].localPosition = new Vector3(
                (currentXPosition - Mathf.Sin(swayPosition))
                 * pixelSize * (i - anchorLayer) * (1 - i * scaleScalar),
                 currentYPosition * pixelSize * (i - anchorLayer) * (1 - i * scaleScalar),
                offsetScalar * i);
        }
    }
}
