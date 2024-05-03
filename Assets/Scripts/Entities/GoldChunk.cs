using UnityEngine;

namespace Entities
{
    public class GoldChunk : RockObstacle
    {
        public override void Hit()
        {
            SpawnGoldPieces();
            HitsToBreak--;
            
            if (HitsToBreak >= 0 && TilemapManager.RemoveObstacle(Cell))
            {
                Destroy(gameObject);
            }
        }

        private void SpawnGoldPieces()
        {
            // var pos = transform.position + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
            var goldPiece = Instantiate(TilemapManager.GoldPiecePrefab, transform.position + Vector3.up, Quaternion.identity);
            var pushDirection = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
            goldPiece.GetComponent<Rigidbody>().AddForce(pushDirection, ForceMode.Impulse);
        }
    }
}
