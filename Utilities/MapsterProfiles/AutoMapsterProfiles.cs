using Black_order.DTOs.CategoryDTOs;
using Black_order.DTOs.ProductDTOs;
using Black_order.Models;
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


        }



    }

}
