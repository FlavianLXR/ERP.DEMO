//using DocumentFormat.OpenXml.Office2010.Excel;
//using Logsys.Geolie3.Blazor.Models.DataAccessLayer;
//using Logsys.Geolie3.Blazor.Models.Files;
//using Logsys.Geolie3.Blazor.Models.Products;
//using Logsys.Geolie3.Blazor.Toolkit.Extensions;
//using Microsoft.AspNetCore.Components.Forms;
//using Microsoft.AspNetCore.Mvc;
//using System.Data.Entity.Validation;
//using System.Web;
//using static System.Runtime.InteropServices.JavaScript.JSType;
//using File = System.IO.File;

//namespace Logsys.Geolie3.Blazor.Components.MVVM
//{
//    public class FileService : BaseService
//    {
//        public FileService(GenericService<GeolieDbContext> geolieService, GenericService<CarrierDbContext> carrierService, LoggerService logger, UserService userService)
//            : base(geolieService, carrierService, logger)
//        {

//        }

//        public List<IFile>? GetFiles(string? productId = null, int? movementId = null)
//        {
//            var db = GeolieDbContext.GetDbContext();
//            if (!string.IsNullOrWhiteSpace(productId))
//            {
//                return db.Products?.Find(productId)?.Files?.Cast<IFile>().ToList();
//            }
//            else if (movementId.HasValue)
//            {
//                return db.Movements?.Find(movementId)?.Files?.Cast<IFile>().ToList();
//            }
//            return null;
//        }

//        public List<IFile>? GetFiles(string directory, string origin, string? productId = null, int? movementId = null)
//        {
//            var db = GeolieDbContext.GetDbContext();
//            List<IFile>? Files = null;
//            if (!string.IsNullOrWhiteSpace(productId))
//            {
//                Files = db.Products?.Find(productId)?.Files?.Cast<IFile>().ToList();
//            }
//            else if (movementId.HasValue)
//            {
//                Files = db.Movements?.Find(movementId)?.Files?.Cast<IFile>().ToList();
//            }

//            if (Files != null && Files.Any(x => x.Directory == directory && x.Origin == origin))
//            {
//                var files = Files.Where(x => x.Directory == directory && x.Origin == origin).ToList();
//                var castedFiles = files.Cast<IFile>().ToList();
//                return castedFiles;
//            }
//            return null;
//        }

//        public FileInfo GetFileInfo(IFile file, IWebHostEnvironment env)
//        {
//            var filePath = GetFilePath(file, env);
//            return new FileInfo(filePath);
//        }

//        public string GetFilePath(IFile file, IWebHostEnvironment env)
//        {
//            var basePath = Path.Combine(env.WebRootPath, file.Directory);
//            var filePath = Path.Combine(basePath, GetFileName(file, (file.Category == "IMG"))).Replace("\\", "/");
//            return filePath;
//        }

//        public string GetFileLocalPath(IFile file)
//        {
//            //return "/ImageArticle/F984561.png"
//            //return "/Fichiers/F984561.pdf"
//            return Path.Combine(file.Directory, GetFileName(file, (file.Category == "IMG"))).Replace("\\", "/");
//        }

//        public string GetFileName(IFile file, bool isImage = false)
//        {
//            if (!isImage)
//            {
//               return ($"F{file.Id}{Path.GetExtension(file.Name)}");
//            }
//            return ($"F{file.Id}.png");
//        }

//        public bool Exist(IFile file, IWebHostEnvironment env)
//        {
//                return File.Exists(GetFilePath(file, env));
//        }

//        public bool Exist(string path)
//        {
//            return File.Exists(path.Replace("\\", "/"));
//        }

//        public async Task Upload(IBrowserFile file, bool isImage, IWebHostEnvironment env, string? productId = null, int? movementId = null)
//        {
//            var product = GeolieDbContext.GetDbContext().Products.Find(productId);
//            var movement = GeolieDbContext.GetDbContext().Movements.Find(movementId);
//            if (file == null)
//                throw new ArgumentNullException(nameof(file), "Le fichier est requis.");

//            if (isImage)
//            {
//                await CreateImageAsync(file, env, productId);
//            }
//            else
//            {
//                await CreateFileAsync(file, env, productId, movementId);
//            }
//        }

//        public async Task CreateFileAsync(IBrowserFile file, IWebHostEnvironment env, string? productId = null, int? movementId = null)
//        {
//            string path = Path.Combine(env.WebRootPath, "Fichiers");

//            if (!Directory.Exists(path))
//                throw new DirectoryNotFoundException($"Le répertoire \"{path}\" n'a pas été trouvé !");

//            var fileName = Path.GetFileName(file.Name);
//            var extension = Path.GetExtension(file.Name);

//            var dbFile = new Models.Files.File
//            {
//                Category = "FIC",
//                CreationDate = DateTime.Now,
//                EmployeeId = -1,
//                Directory = "Fichiers",
//                //Directory = Properties.Settings.Default.FileDirectory,
//                Name = fileName,
//            };

//            if (!string.IsNullOrWhiteSpace(productId))
//            {
//                dbFile.OriginId = productId.Replace("¤", ".");
//                dbFile.Origin = "ART_ID";
//                //dbFile.Origin = Properties.Settings.Default.ProductFileKey;
//            }
//            else if (movementId.HasValue)
//            {
//                dbFile.OriginId = movementId.ToString();
//                dbFile.Origin = "MVT_ID";
//                //dbFile.Origin = Properties.Settings.Default.MovementFileKey;
//            }
//            else
//            {
//                throw new ArgumentException("productId ou movementId requis.");
//            }
//            var db = GeolieDbContext.GetDbContext();
//            db.Files.Add(dbFile);
//            try
//            {
//                await db.SaveChangesAsync();
//            }
//            catch (DbEntityValidationException e)
//            {
//                throw new Exception($"Fichier : { dbFile?.Name } - Erreur de validation des données en base. " + e.Message, e);

//            }
//            catch (Exception e)
//            {
//                throw new Exception($"Fichier : {dbFile?.Name} - Erreur inattendue lors de l'enregistrement en base." + e.Message, e);
//            }

//            try
//            {
//                var savePath = Path.Combine(path, $"F{dbFile.Id}{extension}");
//                using var stream = file.OpenReadStream(long.MaxValue);
//                using var fileStream = System.IO.File.Create(savePath);
//                await stream.CopyToAsync(fileStream);
//            }
//            catch (IOException e)
//            {
//                throw new IOException($"Fichier : {dbFile?.Name} - Erreur lors de l'écriture du fichier sur le disque. Vérifiez les permissions ou l’espace disponible. " + e.Message, e);
//            }
//            catch (Exception e)
//            {
//                throw new Exception($"Fichier : {dbFile?.Name} - Erreur inconnue lors de la copie du fichier. " + e.Message, e);

//            }
//        }

//        public async Task CreateImageAsync(IBrowserFile file, IWebHostEnvironment env, string? productId = null)
//        {
//            if (string.IsNullOrWhiteSpace(productId))
//            {
//                throw new ArgumentException("productId requis.");
//            }

//            var allowedTypes = new[] { "image/png", "image/jpeg", "image/jpg" };
//            if (!allowedTypes.Contains(file.ContentType.ToLower()))
//                throw new InvalidOperationException("Seuls les fichiers PNG ou JPEG sont autorisés.");

//            string path = Path.Combine(env.WebRootPath, "ImageArticle");

//            if (!Directory.Exists(path))
//                throw new DirectoryNotFoundException(string.Format("Le répertoire \"{0}\" n'a pas été trouvé !", path));

//            var fileName = Path.GetFileName(file.Name);

//            var dbFile = new Models.Files.ProductFile
//            {
//                Category = "IMG",
//                CreationDate = DateTime.Now,
//                EmployeeId = -1,
//                Directory = "ImageArticle",
//                //Directory = Properties.Settings.Default.PhotoDirectory,
//                Origin = "ART_ID",
//                //Origin = Properties.Settings.Default.ProductFileKey,
//                Name = fileName,
//                ProductId = productId.Replace("¤", ".")
//            };
//            var db = GeolieDbContext.GetDbContext();
//            db.ProductFiles.Add(dbFile);
//            db.SaveChanges();

//            var savePath = Path.Combine(path, $"F{dbFile.Id}.png");
//            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB max
//            using var fileStream = System.IO.File.Create(savePath);
//            await stream.CopyToAsync(fileStream);
//        }

//        public async Task DeleteFileAsync(int id, bool isImage = false, bool isMovement = false, IWebHostEnvironment env = null)
//        {
//            string basePath = "";
//            try
//            {
//                if (env == null)
//                    throw new ArgumentNullException(nameof(env), "L'environnement WebHost est requis.");
//                var db = GeolieDbContext.GetDbContext();

//                IFile fileDb = null;
//                string filePath = "";

//                if (isImage || !isMovement)
//                {
//                    fileDb = db.ProductFiles.Find(id);
//                    if (fileDb is not ProductFile productFile || fileDb == null)
//                    {
//                        throw new FileNotFoundException($"Fichier produit avec ID {id} introuvable.");
//                    }
//                }
//                else
//                {
//                    fileDb = db.MovementFiles.Find(id);
//                    if (fileDb is not MovementFile movementFile || fileDb == null)
//                    {
//                        throw new FileNotFoundException($"Fichier mouvement avec ID {id} introuvable.");
//                    }
//                }
//                if (isImage)
//                { 
//                    basePath = Path.Combine(env.WebRootPath, "ImageArticle");
//                    filePath = Path.Combine(basePath, $"F{id}.png");

//                    // Supprimer image principale
//                    if (File.Exists(filePath))
//                        File.Delete(filePath);

//                    // Supprimer la miniature
//                    var thumbPath = Path.Combine(basePath, $"F{id}_thumb.png");
//                    if (File.Exists(thumbPath))
//                        File.Delete(thumbPath);

//                    db.ProductFiles.Remove(fileDb as ProductFile);
//                }
//                else
//                {

//                    basePath = Path.Combine(env.WebRootPath, "Fichiers");
//                    var ext = Path.GetExtension(fileDb.Name);
//                    filePath = Path.Combine(basePath, $"F{id}{ext}");

//                    if (File.Exists(filePath))
//                        File.Delete(filePath);

//                    if (isMovement) db.MovementFiles.Remove(fileDb as MovementFile);
//                    else db.ProductFiles.Remove(fileDb as ProductFile);
//                }

//                if (await db.SaveChangesAsync() < 1) {
//                    throw new InvalidOperationException("Erreur lors de la suppression du fichier en base de données.");
//                }
                
//            }
//            catch (DirectoryNotFoundException)
//            {
//                throw new DirectoryNotFoundException($"Le répertoire \"{basePath}\" est introuvable.");
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Une erreur est survenue lors de la suppression du fichier. " + ex.Message, ex);
//            }
//        }

//        public async Task DeleteAllFileAsync(List<int> ids, bool isImage = false, bool isMovement = false, IWebHostEnvironment env = null)
//        {
//            string basePath = "";
//            try
//            {
//                if (env == null)
//                    throw new ArgumentNullException(nameof(env), "L'environnement WebHost est requis.");
//                var db = GeolieDbContext.GetDbContext();
//                foreach (var id in ids)
//                {
//                    IFile fileDb = null;
//                    string filePath = "";

//                    if (isImage || !isMovement)
//                    {
//                        fileDb = db.ProductFiles.Find(id);
//                        if (fileDb is not ProductFile productFile || fileDb == null)
//                        {
//                            throw new FileNotFoundException($"Fichier produit avec ID {id} introuvable.");
//                        }
//                    }
//                    else
//                    {
//                        fileDb = db.MovementFiles.Find(id);
//                        if (fileDb is not MovementFile movementFile || fileDb == null)
//                        {
//                            throw new FileNotFoundException($"Fichier mouvement avec ID {id} introuvable.");
//                        }
//                    }
//                    if (isImage)
//                    {
//                        basePath = Path.Combine(env.WebRootPath, "ImageArticle");
//                        filePath = Path.Combine(basePath, $"F{id}.png");

//                        // Supprimer image principale
//                        if (File.Exists(filePath))
//                            File.Delete(filePath);

//                        // Supprimer la miniature
//                        var thumbPath = Path.Combine(basePath, $"F{id}_thumb.png");
//                        if (File.Exists(thumbPath))
//                            File.Delete(thumbPath);

//                        db.ProductFiles.Remove(fileDb as ProductFile);
//                    }
//                    else
//                    {

//                        basePath = Path.Combine(env.WebRootPath, "Fichiers");
//                        var ext = Path.GetExtension(fileDb.Name);
//                        filePath = Path.Combine(basePath, $"F{id}{ext}");

//                        if (File.Exists(filePath))
//                            File.Delete(filePath);

//                        if (isMovement) db.MovementFiles.Remove(fileDb as MovementFile);
//                        else db.ProductFiles.Remove(fileDb as ProductFile);
//                    }
//                    if (await db.SaveChangesAsync() < 1)
//                    {
//                        throw new InvalidOperationException("Erreur lors de la suppression du fichier en base de données.");
//                    }
//                }
                

//            }
//            catch (DirectoryNotFoundException)
//            {
//                throw new DirectoryNotFoundException($"Le répertoire \"{basePath}\" est introuvable.");
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Une erreur est survenue lors de la suppression du fichier. " + ex.Message, ex);
//            }
//        }
//    }
//}
