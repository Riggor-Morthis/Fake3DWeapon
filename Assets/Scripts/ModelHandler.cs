using UnityEngine;

public class ModelHandler : MonoBehaviour
{
    private Transform[] spriteChild;
    private float scaleScalar = .02f;

    private Camera mainCamera;
    private RotationHandler rotationHandler;

    private Vector3 basicPosition;
    private bool leftClick, rightClick;
    private float positionOffset;
    private Vector2 mouseDelta;
    private float currentXMovement, currentYMovement;
    private float currentXPosition, currentYPosition;

    private float swayLimit = Mathf.PI / 2f, swaySpeed = Mathf.PI;
    private int swayDirection = 1;
    private float swayPosition;
    private float swayXScalar = .05f, swayYScalar = .015f;

    private void Awake()
    {
        SetTheScales();
        InitializeVariables();
    }

    private void LateUpdate()
    {
        CalculateVariables();
        WeaponSway();
        FakeDepth();
    }

    private void SetTheScales()
    {
        //On recupere tous les enfants
        spriteChild = new Transform[transform.childCount];

        //On s'assure qu'ils sont a la bonne taille et qu'ils ont la bonne position pour l'ordre de rendu
        for (int i = 0; i < transform.childCount; i++)
        {
            spriteChild[i] = transform.GetChild(i);
            spriteChild[i].localScale = new Vector3(1 - i * scaleScalar, 1 - i * scaleScalar, 1);
            spriteChild[i].localPosition = Vector3.forward * 0.001f * i;
        }
    }

    private void InitializeVariables()
    {
        //On recupere le script pour les inputs joueurs
        rotationHandler = GetComponentInParent<RotationHandler>();

        //On recupere la camera et on place nos modeles au bon endroit du monde
        mainCamera = Camera.main;
        basicPosition = mainCamera.ScreenToWorldPoint(new Vector3(Mathf.RoundToInt(mainCamera.pixelWidth / 2f), 0, transform.localPosition.z));
        transform.localPosition = basicPosition;

        //On calcule la "projection" d'un pixel du sprite dans les coordonnees monde
        positionOffset = .5f / spriteChild[0].GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
    }

    private void CalculateVariables()
    {
        //On recupere juste les clicks du joueur
        leftClick = rotationHandler.leftClick;
        rightClick = rotationHandler.rightClick;

        //On calcule les angles de la souris en deg/s
        mouseDelta = rotationHandler.GetMouseDelta();
        currentXMovement = Quaternion.Euler(mouseDelta.x, 0f, 0f).x / Time.deltaTime;
        currentYMovement = Quaternion.Euler(0f, mouseDelta.y, 0f).y / Time.deltaTime;
    }

    private void WeaponSway()
    {
        //Si on a un "mouvement"
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
            if (Mathf.Abs(swayPosition) < swaySpeed * 2f * Time.deltaTime) swayPosition = 0;
            //Sinon, on revient au centre
            else swayPosition -= Mathf.Sign(swayPosition) * swaySpeed * 2f * Time.deltaTime;
        }

        //On applique notre position
        transform.localPosition = basicPosition + new Vector3(Mathf.Sin(swayPosition) * swayXScalar, (Mathf.Cos(swayPosition) - 1) * swayYScalar, 0f);
    }

    private void FakeDepth()
    {
        //Si la "camera est en mouvement", on applique ce mouvement a notre "modele"
        if (leftClick)
        {
            currentXPosition -= currentXMovement / 30f;
            currentYPosition += currentYMovement / 15f;
        }

        //Quoi qu'il en soit, on applique un peu de retour a zero, sauf si on est au cap
        if (Mathf.Abs(currentXPosition) > 2) currentXPosition = Mathf.Sign(currentXPosition) * 2;
        else currentXPosition -= Time.deltaTime * Mathf.Sign(currentXPosition);
        if (Mathf.Abs(currentYPosition) > 2) currentYPosition = Mathf.Sign(currentYPosition) * 2;
        else currentYPosition -= Time.deltaTime * Mathf.Sign(currentYPosition);

        //On applique notre position a tous nos sprites (sauf le premier qui est la "racine")
        for (int i = 1; i < spriteChild.Length; i++)
        {
            //On oublie pas d'appliquer le sway en cours a tout moment
            spriteChild[i].localPosition = new Vector3(
                (currentXPosition - Mathf.Sin(swayPosition)) * positionOffset * i * (1 - i * scaleScalar),
                currentYPosition * positionOffset * i * (1 - i * scaleScalar),
                0.001f * i);
        }
    }
}
