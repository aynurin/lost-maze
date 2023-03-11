using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {
    public Sprite dmg75Sprite;
    public Sprite dmg50Sprite;
    public Sprite dmg25Sprite;
    public Sprite dmg00Sprite;
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
        if (hp < float.Epsilon) {
            spriteRenderer.sprite = dmg00Sprite;
            GetComponent<Collider2D>().enabled = false;
        } else if (hp < 0.25f * originalHp) {
            spriteRenderer.sprite = dmg25Sprite;
        } else if (hp < 0.50f * originalHp) {
            spriteRenderer.sprite = dmg50Sprite;
        } else if (hp < 0.95 * originalHp) {
            spriteRenderer.sprite = dmg75Sprite;
        }
    }
}
