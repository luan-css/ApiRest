using System;
using System.Linq;
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using api.HATEOAS;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin")]  
    public class ProdutosController : ControllerBase
    {

        private readonly ApplicationDbContext database;
        private HATEOAS.HATEOAS HATEOAS;

        public ProdutosController(ApplicationDbContext database){
            this.database = database;
            HATEOAS = new HATEOAS.HATEOAS("localhost:5001/api/v1/produtos");
            HATEOAS.AddAction("GET_INFO", "GET");
            HATEOAS.AddAction("DETELE_PRODUCT", "DELETE");
            HATEOAS.AddAction("EDIT_PRODUCT", "PATCH");
        }

        [HttpGet]
        public IActionResult Get(){
            var produtos = database.Produtos.ToList();
            List<ProdutoContainer> produtosHATEOAS = new List<ProdutoContainer>();
            foreach(var prod in produtos){
                ProdutoContainer produtoHATEOAS = new ProdutoContainer();
                produtoHATEOAS.produto = prod;
                produtoHATEOAS.links = HATEOAS.GetActions(prod.Id.ToString());
                produtosHATEOAS.Add(produtoHATEOAS);
            }
            return Ok(produtosHATEOAS);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id){
            try{
            Produto produto = database.Produtos.First(p => p.Id == id);
            ProdutoContainer produtoHATEOAS = new ProdutoContainer();
            produtoHATEOAS.produto = produto;
            produtoHATEOAS.links = HATEOAS.GetActions(produto.Id.ToString());
            return Ok(produtoHATEOAS);
            }
            catch(Exception e){
                Response.StatusCode = 404;
                return new ObjectResult("");
            }

        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id){
            try{
                var produto = database.Produtos.First(p => p.Id == id);
                database.Produtos.Remove(produto);
                database.SaveChanges();
                return Ok(produto);
            }
            catch(Exception e){
                Response.StatusCode = 404;
                return new ObjectResult("");
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProdutoTemp pTemp){

            if(pTemp.Preco <= 0){
                Response.StatusCode = 400;
                return new ObjectResult(new{msg = "O preço do produto nao pode ser menor que 0"});
                
            }
            if(pTemp.Nome.Length <= 1){
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "O  nome do produto precisa ter mais de um caracter."});
            }

            Produto p = new Produto();
            p.Nome = pTemp.Nome;
            p.Preco = pTemp.Preco;
            database.Produtos.Add(p);
            database.SaveChanges();
            Response.StatusCode = 201;
            return new ObjectResult("");
            
        }
        [HttpPatch]
        public IActionResult Patch([FromBody] Produto produto){
            if(produto.Id > 0){
            
            try{
                var p = database.Produtos.First(ptemp => ptemp.Id == produto.Id);
                if(p != null){
                    //editar
                    p.Nome = produto.Nome != null ? produto.Nome : p.Nome;
                    p.Preco = produto.Preco != 0 ? produto.Preco : p.Preco;


                    database.SaveChanges();
                    return Ok();

                }else{
                    Response.StatusCode = 400;
                    return new ObjectResult(new {msg = "Produto nao encontrado"});
                }
            }catch{
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Produto nao encontrado"});
            }
            }else{
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "O id do produto é inválido"});
            }
        }

        public class ProdutoTemp{
            public string Nome{get; set;}
            public float Preco{get; set;}
        }
        public class ProdutoContainer{
            public Produto produto{get; set;}
            public Link[] links{get; set;}
        }
    }
}