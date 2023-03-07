using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {
    public Sprite dmgSprite;
    public float hp = 4;

    public AudioClip chop1;
    public AudioClip chop2;

    private SpriteRenderer spriteRenderer;
    private float originalHp;

    // Start is called before the first frame update
    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalHp = hp;
    }

    // Update is called once per frame
    public void DamageWall(float loss) {
        SoundManager.Instance.RandomizeSfx(chop1, chop2);
        hp -= loss;
        if (hp < 0.9 * originalHp) {
            spriteRenderer.sprite = dmgSprite;
        }
        if (hp <= float.Epsilon) {
            gameObject.SetActive(false);
        }
    }
}
