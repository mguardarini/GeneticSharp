using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using NUnit.Framework;
using Rhino.Mocks;
using TestSharp;
using HelperSharp;
using GeneticSharp.Domain.Randomizations;
using System.Linq;

namespace GeneticSharp.Domain.UnitTests.Crossovers
{
    [TestFixture]
    public class OrderedCrossoverTest
    {
        [TearDown]
        public void Cleanup()
        {
            RandomizationProvider.Current = new BasicRandomization();
        }

        [Test]
        public void Cross_ChromosomeLengthLowerThan3_Exception()
        {
            var target = new OrderedCrossover();
            var chromosome = MockRepository.GenerateStub<ChromosomeBase>(1);

            ExceptionAssert.IsThrowing(new CrossoverException(target, "A chromosome should have, at least, 2 genes. {0} has only 1 gene.".With(chromosome.GetType().Name)), () =>
            {
                target.Cross(new List<IChromosome>() {
                    chromosome,
                    chromosome
                });
            });
        }

        [Test]
        public void Cross_ParentWithNoOrderedGenes_Exception()
        {
            var target = new OrderedCrossover();

            var chromosome1 = MockRepository.GenerateStub<ChromosomeBase>(10);
            chromosome1.ReplaceGenes(0, new Gene[] { 
				new Gene() { Value = 8 },
				new Gene() { Value = 4 },
				new Gene() { Value = 7 },
				new Gene() { Value = 3 },
				new Gene() { Value = 6 },
				new Gene() { Value = 2 },
				new Gene() { Value = 5 },
				new Gene() { Value = 1 },
				new Gene() { Value = 9 },
				new Gene() { Value = 0 }
			});
            chromosome1.Expect(c => c.CreateNew()).Return(MockRepository.GenerateStub<ChromosomeBase>(10));
            
            var chromosome2 = MockRepository.GenerateStub<ChromosomeBase>(10);
            chromosome2.ReplaceGenes(0, new Gene[] 
            { 
				new Gene() { Value = 0 },
				new Gene() { Value = 1 },
				new Gene() { Value = 2 },
				new Gene() { Value = 3 },
				new Gene() { Value = 5 },
				new Gene() { Value = 5 },
				new Gene() { Value = 6 },
				new Gene() { Value = 7 },
				new Gene() { Value = 8 },
				new Gene() { Value = 9 },
            });
            chromosome2.Expect(c => c.CreateNew()).Return(MockRepository.GenerateStub<ChromosomeBase>(10));

            ExceptionAssert.IsThrowing(new CrossoverException(target, "The Ordered Crossover (OX1) can be only used with ordered chromosomes. The specified chromosome has 1 repeated genes."), () =>
            {
                target.Cross(new List<IChromosome>() { chromosome1, chromosome2 });       
            });            
        }

        [Test]
        public void Cross_ParentsWith10Genes_Cross()
        {
            var target = new OrderedCrossover();

			// 8 4 7 3 6 2 5 1 9 0
			var chromosome1 = MockRepository.GenerateStub<ChromosomeBase>(10);
			chromosome1.ReplaceGenes(0, new Gene[] { 
				new Gene() { Value = 8 },
				new Gene() { Value = 4 },
				new Gene() { Value = 7 },
				new Gene() { Value = 3 },
				new Gene() { Value = 6 },
				new Gene() { Value = 2 },
				new Gene() { Value = 5 },
				new Gene() { Value = 1 },
				new Gene() { Value = 9 },
				new Gene() { Value = 0 }
			});
            chromosome1.Expect(c => c.CreateNew()).Return(MockRepository.GenerateStub<ChromosomeBase>(10));

			// 0 1 2 3 4 5 6 7 8 9
            var chromosome2 = MockRepository.GenerateStub<ChromosomeBase>(10);
            chromosome2.ReplaceGenes(0, new Gene[] 
            { 
				new Gene() { Value = 0 },
				new Gene() { Value = 1 },
				new Gene() { Value = 2 },
				new Gene() { Value = 3 },
				new Gene() { Value = 4 },
				new Gene() { Value = 5 },
				new Gene() { Value = 6 },
				new Gene() { Value = 7 },
				new Gene() { Value = 8 },
				new Gene() { Value = 9 },
            });
            chromosome2.Expect(c => c.CreateNew()).Return(MockRepository.GenerateStub<ChromosomeBase>(10));

			// Child one: 0 4 7 3 6 2 5 1 8 9 
            // Child two: 8 2 1 3 4 5 6 7 9 0
            var rnd = MockRepository.GenerateMock<IRandomization>();
            rnd.Expect(r => r.GetInts(2, 0, 10)).Return(new int[] { 3, 7 });
            RandomizationProvider.Current = rnd;

			IList<IChromosome> actual = null;;

			TimeAssert.LessThan (10, () => {
				actual = target.Cross (new List<IChromosome> () { chromosome1, chromosome2 });
			});

            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(10, actual[0].Length);
            Assert.AreEqual(10, actual[1].Length);

            Assert.AreEqual(10, actual[0].GetGenes().Distinct().Count());
            Assert.AreEqual(10, actual[1].GetGenes().Distinct().Count());

            Assert.AreEqual(0, actual[0].GetGene(0).Value);
            Assert.AreEqual(4, actual[0].GetGene(1).Value);
            Assert.AreEqual(7, actual[0].GetGene(2).Value);
            Assert.AreEqual(3, actual[0].GetGene(3).Value);
            Assert.AreEqual(6, actual[0].GetGene(4).Value);
			Assert.AreEqual(2, actual[0].GetGene(5).Value);
			Assert.AreEqual(5, actual[0].GetGene(6).Value);
			Assert.AreEqual(1, actual[0].GetGene(7).Value);
			Assert.AreEqual(8, actual[0].GetGene(8).Value);
			Assert.AreEqual(9, actual[0].GetGene(9).Value);


			Assert.AreEqual(8, actual[1].GetGene(0).Value);
			Assert.AreEqual(2, actual[1].GetGene(1).Value);
			Assert.AreEqual(1, actual[1].GetGene(2).Value);
			Assert.AreEqual(3, actual[1].GetGene(3).Value);
			Assert.AreEqual(4, actual[1].GetGene(4).Value);
			Assert.AreEqual(5, actual[1].GetGene(5).Value);
			Assert.AreEqual(6, actual[1].GetGene(6).Value);
			Assert.AreEqual(7, actual[1].GetGene(7).Value);
			Assert.AreEqual(9, actual[1].GetGene(8).Value);
			Assert.AreEqual(0, actual[1].GetGene(9).Value);
        }
    }
}