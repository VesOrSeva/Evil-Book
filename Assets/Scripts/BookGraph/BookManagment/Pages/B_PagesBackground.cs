using UnityEngine;

namespace BookGraph.Runtime
{
    public class B_PagesBackground : Singleton<B_PagesBackground>
    {
        [SerializeField] Sprite[] backgroundSprites;

        public Sprite GetRandom()
        {
            int R = Random.Range(0, backgroundSprites.Length);
            return backgroundSprites[R];
        }
    }
}
