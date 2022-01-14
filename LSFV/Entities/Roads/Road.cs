using LiteDB;

namespace LSFV
{
    public class Road
    {
        /// <summary>
        /// Gets the id of this <see cref="Road"/>
        /// </summary>
        [BsonId]
        public int Id { get; set; }

        /// <summary>
        /// Gets the name of this <see cref="Road"/>
        /// </summary>
        public string Name { get; set; }
    }
}
