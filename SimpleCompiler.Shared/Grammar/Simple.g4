grammar Simple;


program		: 'public' 'class' ID '{' classBody '}';

classBody	    : decl*;

decl            :  fieldDecl
		        | methodDecl;

fieldDecl	    :  'static' type ID ';'; 

methodDecl	    :  'public' 'static' 'void' ID formals methodBody 
                | 'public' 'static' 'int' ID formals methodBody ;

formals		    :  '(' ')'
		        | '(' formalsList ')';

formalsList	    :  formalDecl (',' formalDecl)*;

formalDecl	    :  type ID	;

methodBody	    :  '{' varDecl* stmt* '}';

varDecl		    :  type ID ';';

switchGroup	    :    switchLabel stmt* ;

switchLabel	    :    'case' exp ','
		        |     'default' ',';
		     
stmt		    :  'print' '(' exp ')' ';'
	            | ID '=' exp ';'
	            | 'if' '(' exp ')' '{' stmt* '}'
	            | 'if' '(' exp ')' '{' stmt* '}' 'else' '{' stmt* '}'
	            | 'do' '{' stmt* '}' 'while' '(' exp ')' 
		        | 'return' ';'
		        | 'return' exp ';'
	            | ID '(' ')' ';' 
	            | ID '(' actualList ')' ';' 
		        | '{' varDecl* stmt* '}'  
		        | 'switch' '(' exp ')' '{' switchGroup* '}';

exp		        : exp '+' exp
	            | exp '-' exp
    	        | exp '*' exp
    	        | exp '/' exp
    	        | '!' exp
    	        | exp '&&' exp
    	        | exp '||' exp
        		| exp '==' exp
	        	| exp '!=' exp
        		| exp '<' exp
	        	| exp '>' exp
        		| exp '<=' exp
        		| exp '>=' exp
        		| exp '**' exp  
	        	| '-' exp
        		| ID
	            | INTLITERAL
	            | STRINGLITERAL
	            | 'true'
	            | 'false' 
	            | '(' exp ')'
	            | ID '(' ')'   	      
	            | ID '(' actualList ')' ;
		
actualList	    :  exp (',' exp)* ; 

type		    :  'int'
		        | 'boolean' 
		        | 'string' ;
		        
		        
ID :    [a-zA-Z_][a-zA-Z0-9_]*;		        
INTLITERAL : [0-9]+;
STRINGLITERAL : '"' ( ~["\\] | '\\' . )* '"';

WS : [ \t\r\n]+ -> skip; 


