using UnityEngine;

namespace Entities
{
    public class GoldOre : RockObstacle
    {
        public override void Hit()
        {
            SpawnGoldPieces();
            base.Hit();
        }

        private void SpawnGoldPieces()
        {
            // var pos = transform.position + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
            var goldPiece = Instantiate(TilemapManager.GoldPiecePrefab, transform.position + Vector3.up, Quaternion.identity);
            var pushDirection = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
            goldPiece.Push(pushDirection);
        }
    }
}
