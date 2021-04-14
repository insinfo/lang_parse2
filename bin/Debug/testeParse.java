package br.com.midiaberta.listaberta.pojos;

public class Country
{
    private Integer id;
   
	public Country(Integer id) 
	{ 
		this.id = id;
	}  

    public String getAbbreviation() {
        return abbreviation;
    }

    public void setAbbreviation(String abbreviation) {
        this.abbreviation = abbreviation;
    }


}
