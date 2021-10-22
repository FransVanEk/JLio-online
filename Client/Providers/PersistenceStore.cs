using JLioOnline.Client.Providers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TG.Blazor.IndexedDB;

namespace JLioOnline.Client.Providers
{

    public class PersistenceStore
    {
        private readonly IndexedDBManager dbManager;

        public PersistenceStore(IndexedDBManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public string StoreName { get; set; } = "JLioModels";

        public async Task<JLioDocumentModel> GetById(string Id)
        {
            return await dbManager.GetRecordById<string, JLioDocumentModel>(StoreName, Id);
        }

        public async Task<bool> Exists(string Id)
        {
            return await dbManager.GetRecordById<string, JLioDocumentModel>(StoreName, Id) != null;
        }

        public async Task Update(JLioDocumentModel model)
        {
            var data = new StoreRecord<JLioDocumentModel>();
            data.Data = model;
            data.Storename = StoreName;

            await dbManager.UpdateRecord<JLioDocumentModel>(data);
        }

        public async Task Delete(string id)
        {
            await dbManager.DeleteRecord(StoreName, id);
        }


        public async Task<bool> Add(JLioDocumentModel model)
        {
            var newRecord = new StoreRecord<JLioDocumentModel>
            {
                Storename = StoreName,
                Data = model
            };

            if (await Exists(model.Id)) { return false; }
            await dbManager.AddRecord(newRecord);
            return true;
        }

        public async Task<List<JLioDocumentModel>> GetModels()
        {
            return await dbManager.GetRecords<JLioDocumentModel>(StoreName);
        }

    }
}
