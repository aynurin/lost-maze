using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {
    public Sprite dmgSprite;
    public int hp = 4;

    public AudioClip chop1;
    public AudioClip chop2;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    public void DamageWall(int loss) {
        SoundManager.Instance.RandomizeSfx(chop1, chop2);
        spriteRenderer.sprite = dmgSprite;
        hp -= loss;
        if (hp <= 0) {
            gameObject.SetActive(false);
        }
    }
}
