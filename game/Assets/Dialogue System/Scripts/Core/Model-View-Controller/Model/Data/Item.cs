namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// An item asset. In Chat Mapper, items are usually used to track the status of items in the 
	/// simulation. You can still do this in the dialogue system; however the QuestLog class gives 
	/// you the option of using the item table to track quest log information instead. (See @ref 
	/// questLogSystem)
	/// </summary>
	[System.Serializable]
	public class Item : Asset {
		
		/// <summary>
		/// Gets or sets the field 'Is Item' which indicates whether this asset is an item
		/// or a quest.
		/// </summary>
		/// <value>
		/// <c>true</c> if asset is actually an item; <c>false</c> if the asset is actually
		/// a quest.
		/// </value>
		public bool IsItem {
			get { return LookupBool("Is Item"); } 
			set { Field.SetValue(fields, "Is Item", value); }
		}

		/// <summary>
		/// Initializes a new Item.
		/// </summary>
		public Item() {}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="sourceItem">Source item.</param>
		public Item(Item sourceItem) : base(sourceItem as Asset) {}

		/// <summary>
		/// Initializes a new Item copied from a Chat Mapper item.
		/// </summary>
		/// <param name='chatMapperItem'>
		/// The Chat Mapper item.
		/// </param>
		public Item(ChatMapper.Item chatMapperItem) {
			Assign(chatMapperItem);
		}
		
		/// <summary>
		/// Copies a Chat Mapper item.
		/// </summary>
		/// <param name='chatMapperItem'>
		/// The Chat Mapper item.
		/// </param>
		public void Assign(ChatMapper.Item chatMapperItem) {
			if (chatMapperItem != null) Assign(chatMapperItem.ID, chatMapperItem.Fields);
		}

	}

}
