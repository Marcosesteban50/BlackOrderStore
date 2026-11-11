using Black_order.DTOs.CategoryDTOs;
using Black_order.DTOs.GuideDTOs;
using Black_order.DTOs.ProductDTOs;
using Black_order.Models;
using Black_order.Models.Guides;
using Mapster;

namespace Black_order.Utilities.MapsterProfiles
{
    public class AutoMapsterProfiles : IRegister
    {


        public void MapsterProfiles(TypeAdapterConfig config)
        {


            Register(config);
        }



        public void Register(TypeAdapterConfig config)
        {

            //*******************************PRODUCTS*******************************/

            //ORIGEN -> DESTINO
            config.NewConfig<Product, ProductDTO>()
            //DESTINO -> ORIGEN
           .Map(productDTO => productDTO.Images, product => product.Images.Select(x => x.Url).ToList())
           .Map(productDTO => productDTO.CategoryName, product => product.Category != null ? product.Category.Name : null);


            config.NewConfig<ProductCreateDTO, Product>().Ignore(product => product.Images);

            config.NewConfig<Product, AddProductsDTO>();
            config.NewConfig<AddProductsDTO, Product>();


            //config.NewConfig<ProductHistory, ProductHistoryDTO>().Map();

            /*******************************CATEGORIES*******************************/
            config.NewConfig<Category, CategoryDTO>()
                .Map(categoryDTO => categoryDTO.Images, category => category.Images.Select(x => x.Url).ToList());

            config.NewConfig<CreateCategoryDTO, Category>()
                .Ignore(x => x.Id)
                .Ignore(x => x.Images)
                .Ignore(category => category.Products);


            /*******************************GUIDES*******************************/
            config.NewConfig<Guide, GuideSummaryDTO>()
                .Map(guideDTO => guideDTO.CategoryName, guide => guide.Category != null ? guide.Category.Name : null)
                .Map(guideDTO => guideDTO.CategorySlug, guide => guide.Category != null ? guide.Category.Slug : null);

            config.NewConfig<Guide, GuideDTO>()
                .Map(guideDTO => guideDTO.CategoryName, guide => guide.Category != null ? guide.Category.Name : null)
                .Map(guideDTO => guideDTO.CategorySlug, guide => guide.Category != null ? guide.Category.Slug : null)
                .Map(guideDTO => guideDTO.Media, guide => guide.Media.OrderBy(media => media.SortOrder).ToList());

            config.NewConfig<GuideMedia, GuideMediaDTO>()
                .Map(mediaDTO => mediaDTO.MediaType, media => media.MediaType.ToString());

            config.NewConfig<GuideCategory, GuideCategoryDTO>();


        }



    }

}
